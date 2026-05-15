using OhMyPackage;
using Launcher;
using UnityEngine;
using ProcedureOwner = OhMyPackage.Fsm.IFsm<OhMyPackage.Procedure.IProcedureManager>;

namespace Procedure
{
    public class ProcedureDownloadOver : ProcedureBase
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public override bool UseNativeDialog { get; }

        private bool _needClearCache;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            Log.Info("下载完成!!!");

            LauncherMgr.ShowUI<LoadUpdateUI>($"下载完成...");

            // 下载完成之后再保存本地版本。
            Utility.PlayerPrefs.SetString("GAME_VERSION", _resourceModule.PackageVersion);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            if (_needClearCache)
            {
                ChangeState<ProcedureClearCache>(procedureOwner);
            }
            else
            {
                ChangeState<ProcedurePreload>(procedureOwner);
            }
        }
    }
}