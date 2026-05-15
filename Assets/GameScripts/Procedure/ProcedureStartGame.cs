using System;
using Cysharp.Threading.Tasks;
using OhMyPackage.Fsm;
using OhMyPackage.Procedure;
using Launcher;

namespace Procedure
{
    public class ProcedureStartGame : ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            StartGame().Forget();
        }

        private async UniTaskVoid StartGame()
        {
            await UniTask.Yield();
            LauncherMgr.HideAllUI();
        }
    }
}