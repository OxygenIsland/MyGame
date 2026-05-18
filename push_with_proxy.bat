@echo off
setlocal

set "HTTP_PROXY=http://127.0.0.1:7897"
set "HTTPS_PROXY=http://127.0.0.1:7897"

echo Using proxy: %HTTP_PROXY%
echo.

:: ── 推送所有子模块 ──────────────────────────────────────────
echo [1/2] Pushing submodules...
git submodule foreach "git push || exit 1"

if errorlevel 1 (
  echo.
  echo Submodule push failed.
  pause
  exit /b 1
)

echo Submodules pushed successfully.
echo.

:: ── 推送主仓库 ─────────────────────────────────────────────
echo [2/2] Pushing main repository...
git push

if errorlevel 1 (
  echo.
  echo Main repository push failed.
  pause
  exit /b 1
)

echo.
echo All pushes succeeded.
pause
