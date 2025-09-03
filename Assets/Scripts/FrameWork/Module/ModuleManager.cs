/**
 * ==========================================
 * Author：xuzq9
 * CreatTime：2023.7.5
 * Description：Manage all modules and controllers
 * ==========================================
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using Stark.Core.Logs;
using StarWorld.Common.Utility;

namespace StarWorld.FrameWork
{
    public static class ModuleFactory
    {
        private static Dictionary<string, Func<IModule>> _moduleFactories =
            new Dictionary<string, Func<IModule>>();
        private static Dictionary<string, Func<IController>> _controllerFactories =
            new Dictionary<string, Func<IController>>();

        public static void RegisterModule<T>(Func<IModule> factory)
            where T : IModule
        {
            _moduleFactories[typeof(T).FullName] = factory;
        }

        public static void RegisterController<T>(Func<IController> factory)
            where T : IController
        {
            _controllerFactories[typeof(T).FullName] = factory;
        }

        public static IModule CreateModule(string typeName)
        {
            if (_moduleFactories.TryGetValue(typeName, out var factory))
            {
                try
                {
                    return factory();
                }
                catch (Exception ex)
                {
                    Log.Error($"[ModuleFactory] Failed to create module {typeName}: {ex.Message}");
                }
            }
            else
            {
                Log.Error($"[ModuleFactory] No factory registered for module {typeName}");
            }
            return null;
        }

        public static IController CreateController(string typeName)
        {
            if (_controllerFactories.TryGetValue(typeName, out var factory))
            {
                try
                {
                    return factory();
                }
                catch (Exception ex)
                {
                    Log.Error(
                        $"[ModuleFactory] Failed to create controller {typeName}: {ex.Message}"
                    );
                }
            }
            else
            {
                Log.Error($"[ModuleFactory] No factory registered for controller {typeName}");
            }
            return null;
        }
    }

    public class ModuleManager : Singleton<ModuleManager>
    {
        private Dictionary<string, ModuleConfig> _moduleConfig =
            new Dictionary<string, ModuleConfig>();
        private Dictionary<string, IModule> _loadedModules = new Dictionary<string, IModule>();
        private Dictionary<string, IController> _loadedControllers =
            new Dictionary<string, IController>();

        public bool TryGetConfig(string name, out ModuleConfig config)
        {
            return _moduleConfig.TryGetValue(name, out config);
        }

        public bool IsModuleLoaded<T>()
        {
            return IsModuleLoaded(typeof(T).FullName);
        }

        public bool IsModuleLoaded(string moduleName)
        {
            return _loadedModules.ContainsKey(moduleName);
        }

        public bool IsControllerLoaded(string moduleName)
        {
            return _loadedControllers.ContainsKey(moduleName);
        }

        public T AcquireModule<T>()
            where T : IModule
        {
            IModule module;
            _loadedModules.TryGetValue(typeof(T).FullName, out module);
            return (T)module;
        }

        public T AcquireController<T>()
            where T : IController
        {
            IController controller;
            _loadedControllers.TryGetValue(typeof(T).FullName, out controller);
            return (T)controller;
        }

        #region public function
        /// <summary>
        /// 读取module配置表
        /// </summary>
        /// <param name="text">xml文本数据</param>
        /// <returns></returns>
        public void ReadModuleConfig(string text)
        {
            XDocument doc = null;
            if (!string.IsNullOrEmpty(text))
            {
                doc = XDocument.Parse(text);
            }
            if (doc == null)
            {
                return;
            }

            XElement pluginRoot = doc.Root;
            IEnumerable<XElement> plugins = pluginRoot.Elements();
            foreach (XElement pitem in plugins)
            {
                ModuleConfig config = new ModuleConfig();
                foreach (XElement pdata in pitem.Elements())
                {
                    if (pdata.Name.LocalName.Equals("full_name"))
                    {
                        config.FullName = pdata.Value;
                    }
                    if (pdata.Name.LocalName.Equals("version"))
                    {
                        config.Version = pdata.Value;
                    }
                    if (pdata.Name.LocalName.Equals("description"))
                    {
                        config.Descript = pdata.Value;
                    }
                    if (pdata.Name.LocalName.Equals("subModules"))
                    {
                        config.SubModules = new List<string>();
                        foreach (var submodule in pdata.Elements())
                        {
                            if (!string.IsNullOrEmpty(submodule.Value))
                            {
                                config.SubModules.Add(submodule.Value);
                            }
                        }
                    }
                    if (pdata.Name.LocalName.Equals("controllers"))
                    {
                        config.Controllers = new List<string>();
                        foreach (var controller in pdata.Elements())
                        {
                            if (!string.IsNullOrEmpty(controller.Value))
                            {
                                config.Controllers.Add(controller.Value);
                            }
                        }
                    }
                }
                _moduleConfig[config.FullName] = config;
            }
        }

        public WaitingForJobDone Load<T>(object data = null)
            where T : IModule, new()
        {
            return Load(typeof(T), data);
        }

        public WaitingForJobDone Load(string moduleName, Assembly assembly, object data = null)
        {
            if (assembly != null)
            {
                return Load(assembly.GetType(moduleName), data);
            }
            else
            {
                return Load(Type.GetType(moduleName), data);
            }
        }

        public WaitingForJobDone Load(Type type, object data)
        {
            WaitingForJobDone moduleCustomYield = new WaitingForJobDone();
            string moduleName = type.FullName;
            if (_loadedModules.ContainsKey(moduleName))
            {
                Log.Warn($"[ModuleManager] [{moduleName}] has been loaded...");
                moduleCustomYield.Done();
                return moduleCustomYield;
            }

            var config = GetModuleConfig(moduleName);
            if (config == null)
            {
                Log.Error($"[ModuleManager] ModuleConfig can not find [{moduleName}]");
                moduleCustomYield.Done();
                return moduleCustomYield;
            }

            List<IEnumerator> corutions = new List<IEnumerator>();

            // 使用工厂创建模块
            IModule module = ModuleFactory.CreateModule(moduleName);
            if (module == null)
            {
                Log.Error($"[ModuleManager] Failed to create module [{moduleName}]");
                moduleCustomYield.Done();
                return moduleCustomYield;
            }

            // create controllers
            List<string> controller_list = new List<string>();
            GetAllControllersInModule(moduleName, controller_list, false);
            List<IController> controllers = CreateControllers(controller_list);

            Log.Info($"[ModuleManager] Begin to load module:[{moduleName}]...");
            module.Preload();
            // load
            foreach (var item in controllers)
            {
                string controller_type = item.GetType().Name;
                corutions.Add(
                    WrapIEnumerator(
                        () =>
                        {
                            Log.Info(
                                $"[ModuleManager] Begin to load controller:[{controller_type}]..."
                            );
                        },
                        () =>
                        {
                            Log.Info(
                                $"[ModuleManager] End to load controller:[{controller_type}]..."
                            );
                        },
                        item.OnStart()
                    )
                );
            }
            corutions.Add(
                WrapIEnumerator(
                    null,
                    () =>
                    {
                        _loadedModules.Add(moduleName, module);
                        moduleCustomYield.Done();
                        Log.Info($"[ModuleManager] End to load module:[{moduleName}]...");
                    },
                    module.OnLoad(data)
                )
            );
            CoroutineHelper.Instance.Sequence(corutions);
            return moduleCustomYield;
        }

        /// <summary>
        /// 卸载Module
        /// 执行顺序：controller.OnStop()->module.OnUnload()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public WaitingForJobDone Unload<T>()
            where T : IModule, new()
        {
            return Unload(typeof(T).FullName);
        }

        public WaitingForJobDone Unload(Type type)
        {
            return Unload(type.FullName);
        }

        public WaitingForJobDone Unload(string moduleName)
        {
            WaitingForJobDone moduleCustomYield = new WaitingForJobDone();
            if (!_loadedModules.ContainsKey(moduleName))
            {
                Log.Warn($"[ModuleManager] [{moduleName}] has not been loaded...");
                moduleCustomYield.Done();
                return moduleCustomYield;
            }

            var config = GetModuleConfig(moduleName);
            if (config == null)
            {
                Log.Error($"[ModuleManager] ModuleConfig can not find [{moduleName}]");
                moduleCustomYield.Done();
                return moduleCustomYield;
            }

            List<IEnumerator> corutions = new List<IEnumerator>();
            // unload module
            IModule module;
            _loadedModules.TryGetValue(moduleName, out module);
            _loadedModules.Remove(moduleName);

            // stop controllers
            List<string> controller_list = new List<string>();
            GetAllControllersInModule(moduleName, controller_list, false);
            List<IController> controllers = StopControllers(controller_list);

            // do unload
            module.PreUnload();
            Log.Info($"[ModuleManager] Begin to unload module:[{moduleName}]...");
            foreach (var item in controllers)
            {
                string controller_type = item.GetType().Name;
                corutions.Add(
                    WrapIEnumerator(
                        () =>
                        {
                            Log.Info(
                                $"[ModuleManager] Begin to unload controller:[{controller_type}]..."
                            );
                        },
                        () =>
                        {
                            Log.Info(
                                $"[ModuleManager] End to unload controller:[{controller_type}]..."
                            );
                        },
                        item.OnStop()
                    )
                );
            }
            corutions.Add(
                WrapIEnumerator(
                    null,
                    () =>
                    {
                        moduleCustomYield.Done();
                        Log.Info($"[ModuleManager] End to unload module:[{moduleName}]...");
                    },
                    module.OnUnload()
                )
            );
            CoroutineHelper.Instance.Sequence(corutions);

            return moduleCustomYield;
        }
        #endregion

        /// <summary>
        /// 获取module的config
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private ModuleConfig GetModuleConfig(string name)
        {
            ModuleConfig config = null;
            _moduleConfig.TryGetValue(name, out config);
            return config;
        }

        /// <summary>
        /// 启动controller
        /// </summary>
        /// <param name="list"></param>
        /// <param name="assembly"></param>
        private List<IController> CreateControllers(List<string> list)
        {
            List<IController> controllers = new List<IController>();
            foreach (var item in list)
            {
                if (!_loadedControllers.ContainsKey(item))
                {
                    IController controller = ModuleFactory.CreateController(item);
                    if (controller != null)
                    {
                        _loadedControllers.Add(item, controller);
                        RegestMsgForController(controller);
                        controllers.Add(controller);
                    }
                }
            }
            return controllers;
        }

        /// <summary>
        /// 停止controller
        /// </summary>
        /// <param name="list"></param>
        private List<IController> StopControllers(List<string> list)
        {
            List<IController> controllers = new List<IController>();
            foreach (var item in list)
            {
                IController controller;
                if (_loadedControllers.TryGetValue(item, out controller))
                {
                    controllers.Add(controller);
                    _loadedControllers.Remove(item);
                    UnRegestMsgForController(controller);
                }
            }

            return controllers;
        }

        /// <summary>
        /// 注册controller的消息列表
        /// </summary>
        /// <param name="controller"></param>
        private void RegestMsgForController(IController controller)
        {
            var msglist = controller.MsgList;
            if (msglist == null)
            {
                return;
            }
            foreach (var msgid in msglist)
            {
                MsgManager.Instance.Regist(msgid, controller.HandMessage);
            }
        }

        /// <summary>
        /// 注销controller的消息列表
        /// </summary>
        /// <param name="controller"></param>
        private void UnRegestMsgForController(IController controller)
        {
            var msglist = controller.MsgList;
            if (msglist == null)
            {
                return;
            }
            foreach (var msgid in msglist)
            {
                MsgManager.Instance.UnRegist(msgid, controller.HandMessage);
            }
        }

        /// <summary>
        /// 获取module下所有的controller,包涵submodule
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="controllerList"></param>
        /// <param name="recursion">是否递归submodule</param>
        private void GetAllControllersInModule(
            string moduleName,
            List<string> controllerList,
            bool recursion
        )
        {
            var config = GetModuleConfig(moduleName);
            if (config == null)
            {
                Log.Error($"[ModuleManager] ModuleConfig can not find [{moduleName}]");
                return;
            }

            if (recursion)
            {
                foreach (var item in config.SubModules)
                {
                    GetAllControllersInModule(item, controllerList, recursion);
                }
            }

            controllerList.AddRange(config.Controllers);
        }

        /// <summary>
        /// 获取root module下的所有module,包涵其下的所有submodule
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="moduleList"></param>
        private void GetAllModuleByRoot(string moduleName, List<string> moduleList)
        {
            var config = GetModuleConfig(moduleName);
            if (config == null)
            {
                Log.Error($"[ModuleManager] ModuleConfig can not find [{moduleName}]");
                return;
            }

            if (config.SubModules != null)
            {
                foreach (var item in config.SubModules)
                {
                    GetAllModuleByRoot(item, moduleList);
                }
            }

            moduleList.Add(moduleName);
        }

        private IEnumerator WrapIEnumerator(Action before, Action after, IEnumerator enumerator)
        {
            before?.Invoke();
            yield return enumerator;
            after?.Invoke();
        }

        //public void Pause(string modulename)
        //{
        //    Module module = null;
        //    if (!subymodules.TryGetValue(modulename, out module))
        //    {
        //        Log.Error($"[ModuleManager] module:{modulename} has not been loaded...");
        //        return;
        //    }

        //    if (pausedModules.ContainsKey(modulename))
        //    {
        //        Log.Error($"[ModuleManager] module:{modulename} has been paused...");
        //        return;
        //    }

        //    module.submodule.OnPause();
        //    pausedModules.Add(modulename, module);
        //}

        //public void PauseByCategory(string category)
        //{
        //    foreach (var item in subymodules)
        //    {
        //        if (item.Value.descripe.Category.Equals(category))
        //        {
        //            Pause(item.Value.descripe.Name);
        //        }
        //    }
        //}

        //public void Resume(string modulename)
        //{
        //    Module module = null;
        //    if (!subymodules.TryGetValue(modulename, out module))
        //    {
        //        Log.Error($"[ModuleManager] module:{modulename} has not been loaded...");
        //        return;
        //    }

        //    if (!pausedModules.ContainsKey(modulename))
        //    {
        //        Log.Error($"[ModuleManager] module:{modulename} has not been paused...");
        //        return;
        //    }

        //    module.submodule.OnResume();
        //    pausedModules.Remove(modulename);
        //}

        //public void ResumeyCategory(string category)
        //{
        //    foreach (var item in subymodules)
        //    {
        //        if (item.Value.descripe.Category.Equals(category))
        //        {
        //            Resume(item.Value.descripe.Name);
        //        }
        //    }
        //}
    }
}
