namespace StarWorld
{
    public enum MsgID
    {
        //------------系统保留0~100--------------
        #region system
        None = 0,

        //--------------Unity&web Start-----------------
        #region system for Unity&web
        OnFullScreen, // 进入全屏模式,Unity -> Web
        OnFullScreenResponse, //进入全屏模式反馈，Web-> Unity
        OnFullScreenExit, //退出全屏模式,Unity -> Web
        OnFullScreenExitResponse, //退出全屏模式反馈，Web-> Unity

        /// <summary>
        /// 用于动态传递网页配置更换，如语言，应用名称等
        /// {"language":"CN/EN","appName":""}
        /// </summary>
        OnAppConfigChange,
        OnWebFullScreenExit, //web主动按键退出全屏模式, Web -> Unity
        OnWebFullScreenEnter, //web主动按键退出全屏模式, Web -> Unity
        OnGetFullScreenStatus, //Unity请求全屏状态,Unity -> Web

        //1.key:"status", value: 0://非全屏，1，全屏
        OnGetFullScreenStatusResponse, //Web反馈全屏状态，Web -> Unity
        OnOpenMetaverseOperation, //从构建打开运营平台，暂时不传递参数

        /// <summary>
        /// 从构建新打开一个页签，Unity -> Web
        /// {"url":xx"}
        /// </summary>
        OnOpenUrl,

        /// <summary>
        /// 一些Unity告诉Web端的指令，Unity -> Web
        /// 如下，告诉Web端复制一个Url到粘贴板
        /// {
        ///     "Operation":"Clipboard",
        ///     "OperationValue","https://dev-starworld-service-web.lenovo-r.cloud"
        /// }
        /// </summary>
        OnWebOperate,
        #endregion
        //--------------Unity&web End-----------------

        OnCloudRenderConnected, //云渲染连接成功
        OnCloudRenderDisconnected, //云渲染断开连接
        #endregion

        //-----------全局模组保留 101~ 999-----------
        #region global

        //--------------Unity&web Start-----------------
        /****
         * 消息体：
         * {
            “token”:"74c524fd-3de5-4436-a21d-432b3f56b115"
            }
         */
        Global_OnTokenRefreshed = 101, //web -> Unity 收到
        Global_OnTokenRefreshSuccessed, // 当接收到Global_OnTokenRefreshed后，并成功获取新token会发送该命令
        Global_AppReady, // unity -> web unity加载后发送

        /****
       * 消息体：
       * {
       *   "mspId":string       //场景id
         }
       */

        Global_LoadRobotContentEnd, // 机器人孪生体加载完成

        Global_Application_Pause, // web -> unity 挂起Unity
        Global_Application_Resume, // web -> unity 恢复Unity

        // 当web端退出登录后调用
        Global_OnLogOut, //web -> Unity 收到

        /// <summary>
        /// Token失效，web不同端登录弹出框时会方式这个
        /// </summary>
        Global_OnTokenInvalid, //web -> Unity 收到

        /****
         * 消息体：
         * {
            “jsonString”:""
            }
         */
        Global_OnReceiveServerPush, // signal server -> web (服务器推送的数据)
        Global_OnChannelConnectionStart, //对端链接数据通道
        Global_OnChannelConnectionStop, //用户断开云渲染

        //--------------Unity&web End-----------------

        Global_SwitchMainState, // 切换主状态机的状态
        Global_OnMainStateChanged, // 主状态的切换
        Global_OnAssetIconClicked, //水滴icon被点击
        Global_OnAssetIconsClicked, //单点多任务的水滴icon被点击
        Global_OnAssetIconDoubleClicked, //水滴icon被双击击

        Global_CreateTaskTemplate, //新建任务模板，给左侧场景树等做处理
        Global_DropTaskTemplate, //丢弃放弃，处于编辑态的任务点，任务点controller需要自己去释放beu、去除icon等操作

        /****
         * 消息体：
         * {
            “message”: string
            "progress":float            // 进度
            }
        */
        Global_ShowWebLoadingPage, //显示web端的loading界面   unity->web
        Global_HideWebLoadingPage, //关闭web端loading界面  unity->web
        Global_HideWebLoadingPage_ACK, //收到关闭web端loading界面的确认  web->unity
        Global_ConnectServerPush, // 开始连接服务器消息推送
        Global_DisconnectServerPush, // 关闭连接服务器消息推送
        Global_AddSubscribe, // 注册消息推送
        Global_RemoveSubscribe, // 删除消息推送注册
        Global_ClearSubscribe, // 清除所有的消息推送注册

        /// <summary>
        /// useweb: bool        true:使用web端的loading面板  false: 使用unity自己的，默认为false，
        /// message: string     需要显示的tips信息
        /// texture: Texture    背景图(如果传空，使用默认背景)
        /// showBG: bool        是否显示背景图
        /// msgList: List<MsgInfo>     多段提示文字
        /// </summary>
        Global_LoadingView_FullScreenTip_Show,
        Global_LoadingView_FullScreenTip_Hide,

        /// <summary>
        /// 遮罩退出
        /// time: float         持续时间
        /// endValue: float     目标结束值
        /// texture: Texture    背景图(如果传空，使用默认黑色背景)
        /// replay: bool     UI可以受线性流程的影响进行播放（无需先进行MaskIn）
        /// onCompleted: Action 完成后的回调
        /// </summary>
        Global_LoadingView_CameraOverlayer_MaskOut,

        /// <summary>
        /// 遮罩覆盖
        /// time: float         持续时间
        /// endValue: float     目标结束值
        /// texture: Texture    背景图(如果传空，不覆盖当前背景，使用Fadeout时的背景图)
        /// onCompleted: Action 完成后的回调
        /// </summary>
        Global_LoadingView_CameraOverlayer_MaskIn,

        /// <summary>
        /// 修改当前遮罩视图
        /// texture: Texture    背景图
        /// </summary>
        Global_LoadingView_CameraOverlayer_ChangeMaskImageOnly,

        /// <summary>
        /// time: float         持续时间
        /// endValue: float     目标结束值
        /// texture: Texture    背景图(如果传空，使用默认黑色背景)
        /// onCompleted: Action 完成后的回调
        /// </summary>
        Global_LoadingView_CameraOverlayer_Fadeout,

        /// <summary>
        /// time: float         持续时间
        /// endValue: float     目标结束值
        /// texture: Texture    背景图(如果传空，不覆盖当前背景，使用Fadeout时的背景图)
        /// replay: bool     UI可以受线性流程的影响进行播放（无需先进行Fadeout）
        /// onCompleted: Action 完成后的回调
        /// </summary>
        Global_LoadingView_CameraOverlayer_Fadein,

        Global_SetCameraView_Immediately,

        /// <summary>
        /// focus:Vector3       聚焦位姿
        /// distance： float    聚焦距离
        /// </summary>
        Global_CameraView_FocusTo, //聚焦
        Global_CameraView_EnableWASD, //启用WASD
        Global_CameraView_DisableWASD, //禁用WASD

        /// <summary>
        /// NeRF 地图数据开始加载
        /// </summary>
        //Global_NeRF_Loading_Begin,//舍弃

        /// <summary>
        /// NeRF 地图数据加载成功
        /// </summary>
        Global_NeRF_Loading_Successed,

        /// <summary>
        /// NeRF 地图数据加载失败
        /// errorCode : NerfLoadingError(Enum)
        /// </summary>
        Global_NeRF_Loading_Error,

        /// <summary>
        /// 获取屏幕捕获工具操作句柄
        /// on_get_handle : System.Action<IScreenCapturer> 获取屏幕快照操作句柄
        /// </summary>
        Global_ScreenCapture_GetCapturerHandle,

        /// <summary>
        /// 激活自动屏幕快照功能
        /// </summary>
        Global_ScreenCapture_EnableAutoCapturer,

        /// <summary>
        /// 关闭自动屏幕快照功能
        /// </summary>
        Global_ScreenCapture_DisableAutoCapturer,

        /// <summary>
        /// 获取屏幕自动捕获工具操作句柄
        /// on_get_handle : System.Action<IAutoScreenCapturer> 获取自动屏幕快照操作句柄
        /// </summary>
        Global_ScreenCapture_GetAutoCapturerHandle,

        /// <summary>
        /// 全局性能消息
        /// </summary>
        Global_PerformanceAnalyzer_SystemInfo,

        /// <summary>
        /// "uuid": String
        /// "type": Enum(Integer)::0:None::1:Image::2:Video::100:All
        /// </summary>
        Global_UploadMediaAsset,

        /// <summary>
        /// "uuid": String
        /// "state": Enum(Integer)::0:None::1:Succeed::2:Failed::3:Canceled
        /// "error_message": String
        /// "file_name": String::xxx.png
        /// "file_url": String
        /// "thumbnail_url": String
        /// </summary>
        Global_MediaAssetUploadingResult,

        /*
         * 消息体：
         * {
         *      uuid :string     弹出框唯一id
         *      title:string     弹窗标题
         *      url: string      图片/视频/web
         *      content_type:    0,1,2
         *      width:float      弹窗宽度:大于1是绝对坐标，反之相对坐标，<=0默认坐标
         *      height:float     弹窗高度
         *      anchor_x:float    中心点x坐标:左上角为（0,0）
         *      anchor_y:float    中心点y坐标
         *      showAnchorFlag:bool
         *      position_x: float   弹窗中心点坐标: 相对坐标，左上角（0,0）
         *      position_y: float   弹窗中心点坐标
         *      showCloseBtn:bool  是否显示关闭按钮
         *
         * }
         */
        Global_ShowWebPopup, //打开web弹窗

        /*
         * 消息体：
         * {
         *      uuid :string        弹出框唯一id
         * }
         */
        Global_ClosePopup, //关闭web弹窗

        /// <summary>
        /// "uuid": String     web -> unity
        /// </summary>
        Global_POI_ClosePopup,

        /// <summary>
        /// WebGL分辨率切换，适配nerf
        /// 消息体：
        /// {
        ///     width: int     nerf texture 的宽
        ///     height: int    nerf texture 的高
        /// }
        /// </summary>
        Global_Resolution_Changed, // WebGL分辨率切换完成后，通知其他模块

        // 发送端带宽估计
        Global_BWE_Value, // web监听该消息获取值
        Global_Start_BWE, // web发送该消息开启该功能
        Global_Stop_BWE, // web发送该消息关闭该功能

        /*
         * 消息体：
         * {
         *      sh_degree :int        球谐阶数
         * }
         */
        Global_NeRF_SetRenderingQuality,

        /***
         * latency: 0 ms,  //网络延迟
            resolution: '0x0', //分辨率
            framerate: 0, //运行帧率
            bitrate: 0, //传输速率
            bandwidth: 0, //带宽
            level: 'normal',  //网络状态（optimal/normal/poor）
         */
        Global_NetworkState, //web ->Unity,  网络状态信息
        Global_OpenNetworkState, //Unity->web,  打开网络状态面板

        Global_GenerateExternalCollider_Free,

        /*
         * in:
            "points" : float array[]
            out:
                "guid" : string or long
         */
        //Global_GenerateExternalCollider_ConvexHull,//舍弃

        /*
         * in:
            "points" : float array[]
            out:
                "guid" : string or long
         */
        Global_GenerateExternalCollider_ConvexHull_ByVerticalHight,

        /*
         * in:
            "points" : float array[]
            out:
                "guid" : string or long
         */
        Global_RebuildExternalCollider_ConvexHull_ByVerticalHight,

        /*
         * in:
            "guid" : string or long
         */
        Global_DestroyExternalCollider_Free,
        Global_DestroyExternalCollider_ConvexHull,
        Global_DestroyExternalCollider_All,

        /*
         * in:
            "self_layer" : string
            "targets_laryer" : string[]
         */
        Global_ChangeExternalColliderMask,

        /*
         * in:
            "type" : ExternalVerticalType
            "result" : Action<float>
         */
        Global_GetExternalColliderVerticalHight,

        /*
         * in:
            "type" : ExternalVerticalType
            "result" : Action<float[2]>
         */
        Global_GetExternalColliderVerticalRange,

        /*
         * in:
            "result" : Action<float>
         */
        Global_GetExternalColliderWallScaleRange,

        /*
         * in:
            "enable" : bool
         */
        Global_SwitchExternalColliderViewState_ConvexHull,

        /// <summary>
        /// {"mspID": int}
        /// </summary>
        Global_OpenGlobalSettings, //Unity->web，打开全局设置面板

        Global_OpenHelpDocument, //Unity->web，打开帮助面板

        Global_CloseWebDialog, //->web->Unity，关闭当前web面板

        /// <summary>
        /// 获取导览视图器实例
        /// "callback":Action<IguidePathPlayable>
        /// </summary>
        Global_Get_GuideView_Handler,

        /// <summary>
        /// "orientation": int         0:横屏 1：竖屏
        /// </summary>
        Global_OnScreen_OrientationChanged, // 横竖屏切换  web->unity

        /// <summary>
        /// "name":string
        /// "value":string
        /// </summary>
        Global_OnSettingChanged, // 用户设置发生变化

        /// <summary>
        /// "visible":bool
        /// </summary>
        Global_OnCursorChanged, // web端鼠标显示或隐藏 Unity →web

        /****
        * 消息体：
        * {
        “drawing”:true，//源列表
        }
        */
        Global_NeRF_RenderingStateChanged,

        Global_OpenDistanceMearsure, // 开启测距    web->unity
        Global_OnDistanceMearsureOpened, // 测距开启    Unity →web
        Global_CloseDistanceMearsure, // 关闭测距   web->unity
        Global_OnDistanceMearsureClosed, // 测距关闭    Unity →web

        Global_AppStatusRequest, //web -> unity, 获取APP状态

        /****
        * 消息体：
        * {
        stage: AppReady/EditMsp/ViewMsp
        isSwitching:  true false//是否在切换状态，一般用于场景加载
        errorCode: string
        message: string
        mspID: string
        progress: float
        }
        */
        Global_AppStatusResponse, //Unity -> web, 反馈APP状态

        Global_AI_Service_Response, // AI意图识别回复

        Global_Navigation_Service_Start, // 导航服务初始化

        /// <summary>
        /// map: Mesh[]
        /// </summary>
        Global_Navigation_Service_Update, // 导航数据更新：数字底板、楼层
        Global_Navigation_Service_Stop, // 导航服务停止

        /// <summary>
        /// strategy: int       寻路策略
        /// agent: json object
        /// {
        ///   “radius”:float  //半径
        ///   “height”:float  //高度
        ///   “speed”:float  //最大速度
        ///}
        /// "points": vector3[]          // 目标点序列,0:起点   lenght-1：终点
        /// </summary>
        Global_Navigation_Service_Request, // 导航任务请求

        /// <summary>
        /// "path_points": vector3[]  路径点序列
        /// "report":json             路径评分信息
        /// </summary>
        Global_Navigation_Service_Response, // 导航任务请求回复

        GLobal_Start_Microphone_Record, // 开启麦克风音频录制   unity->web
        Global_Stop_Microphone_Record, // 停止麦克风音频录制   unity->web

        /// <summary>
        /// "data": string  wav格式转base64字符串
        /// </summary>
        Global_Response_Microphone_Record, // 麦克风音频录制数据回复   web->unity

        /****
         * 消息体：
         * {
            “mode”: 0 //自由漫游模式（行走），1//浏览模式（沙盘），2//预定轨迹模式，3//机器人视角模式，4//AR视角模式 , 5//2d模式
            "target":Transform  //目标点
            "offset":Vector3  //偏移量
            "callback":Action   //切换完成后的回调,
            }
         */
        Global_SwitchCameraMode, //切換视野操控模式
        Global_OnCameraModeChanged, // 視角模式切換完成

        Global_EnableHDImagePanel, //启用高清图像面板
        Global_DisableHDImagePanel, //禁用高清图像面板

        /***
         * 消息体：
         * {
         *      imagePath :string     高清图像路径
         * }
         */
        Global_ShowHDImagePreview, //显示高清图像预览
        Global_HideHDImagePreview, //隐藏高清图像预览
        #endregion

        //-----------云渲染远程输入保留 5101~ 5300 -----------
        #region RemoteInput
        DataChannel_LeftAlt_Up = 5101,
        DataChannel_LeftAlt_Down = 5102,
        #endregion

        #region  Robot controller
        RobotCtrl_EnterScene, // 机器人遥操作进入主场景,此时孪生体还未加载

        //   RobotCtrl_OnDeviceConnected, // 设备连接成功
        //   RobotCtrl_OnDeviceDisconnected, // 设备断开连接

        /// <summary>
        /// callback: Action<bool>  true-成功，false-失败
        /// </summary>
        RobotCtrl_PointToWalk_Start, // 开启指点行走
        RobotCtrl_PointToWalk_Stop, // 停止指点行走
        RobotCtrl_PointToWalk_Interrupt, // 打断当前指点行走的执行
        RobotCtrl_PointToWalk_OnOpened, // 指点行走打开
        RobotCtrl_PointToWalk_OnStopped, // 指点行走退出
        RobotCtrl_PointToWalk_ShowMask, // 显示遮罩
        RobotCtrl_PointToWalk_HideMask, // 隐藏遮罩
        RobotCtrl_PointToWalk_Cancel, // 取消至某坐标点

        RobotCtrl_OpenTaskDataPanel, // 打开任务数据面板

        /// <summary>
        /// "popupType": int  0-default,1-用户管理，2-设备管理，3-license管理     unity -> web
        /// </summary>
        RobotCtrl_OpenManagePopup,

        /// <summary>
        /// previewType:PreviewType
        /// callback: Action<bool>  true-成功，false-失败
        /// </summary>
        RobotCtrl_SwitchPreview, // 切换预览画面

        /// <summary>
        /// previewType:PreviewType
        /// </summary>
        RobotCtrl_OnPreviewSwitched, // 预览画面切换

        RobotCtrl_EnablePreviewSwitch, // 启用预览切换
        RobotCtrl_DisablePreviewSwitch, // 禁用预览切换

        /// <summary>
        /// callback: Action<PreviewType>
        /// </summary>
        RobotCtrl_OnGetCurrentPreview, // 获取当前预览信息

        RobotCtrl_RefreshTempTaskData, // 刷新临时任务数据

        RobotCtrl_View_OpenBGRender, // 打开背景渲染
        RobotCtrl_View_CloseBGRender, // 关闭背景渲染

        /// <summary>
        /// mask: int operation:string
        /// </summary>
        RobotCtrl_View_UpdateCullMask, // 更新相机裁剪层

        /// <summary>
        /// 退出登录     unity -> web
        /// </summary>
        RobotCtrl_Logout,

        RobotCtrl_UpgradeRobot_Cloud, // 检查云端机器人固件版本更新
        RobotCtrl_UpgradeRobot_Local, // 检查本地机器人固件版本更新

        RobotCtrl_SDK_Connected, // sdk连接成功
        RobotCtrl_SDK_ConnectFailed, // sdk连接失败
        RobotCtrl_SDK_Disconnected, // sdk断开连接
        RobotCtrl_Perception_Connected, // 感知连接成功
        RobotCtrl_Perception_ConnectFailed, // 感知连接失败
         RobotCtrl_Perception_Disconnected, // 感知断开连接

        RobotCtrl_StartUp, //开机动画播放完毕
        RobotCtrl_Wifi_Ready, // wifi 连接正常
        RobotCtrl_Wifi_NotReached, // wifi 未连接

        RobotCtrl_Capture_Picture, //拍照
        RobotCtrl_Capture_Video_Begin, // 视频录制开始
        RobotCtrl_Capture_Video_End, // 视频录制结束
        RobotCtrl_Saved_Picture, // 拍照保存成功
        RobotCtrl_RefreshJoystickPanel,//刷新joystickPanel

        RobotCtrl_SDK_OnEnableError, // 上使能失败
        RobotCtrl_SDK_OnRobotEnabled, // 上使能成功
        RobotCtrl_SDK_OnRobotDisable, // 下使能成功
        /// <summary>
        /// "directions": int         -1:Previous 1：Next
        /// </summary>
        RobotCtrl_Preview_ScrollPreview, // 滚动预览图
        RobotCtrl_SDK_OnPreviewSwitching, // 预览切换中
        RobotCtrl_SDK_OnPreviewSwitched, // 预览切换完成

        RobotCtrl_Preview_Select, // 预览选择
        RobotCtrl_Preview_DeSelect, // 预览取消选择
        #endregion
    }
}
