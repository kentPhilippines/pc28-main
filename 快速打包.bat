@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ====================================
echo  PC28机器人快速打包脚本
echo ====================================
echo.

echo 当前工作目录: %~dp0
echo.

REM 检查是否在子目录中有项目
if exist "pc28-main" (
    echo 在 pc28-main 子目录中发现项目，正在切换...
    cd /d "pc28-main"
    echo 已切换到项目目录
    echo.
)

REM 动态查找解决方案文件而不是硬编码文件名
dir *.sln >nul 2>&1
if %errorlevel% neq 0 (
    echo 错误: 未找到解决方案文件！
    echo 目录内容:
    dir /b
    echo.
    pause
    exit /b 1
)

echo 在当前目录找到解决方案文件
echo.

echo [1/8] 检查并停止正在运行的程序...

REM 尝试停止可能正在运行的程序
echo 正在停止可能运行的PC28机器人程序...
taskkill /F /IM "PC28机器人.exe" >nul 2>&1
taskkill /F /IM "PC28智能托.exe" >nul 2>&1

REM 等待进程完全退出
echo 等待进程完全退出...
timeout /t 3 /nobreak >nul
echo 进程检查完成

echo [2/8] 清理旧的发布目录...
if exist "发布版本" (
    rmdir /s /q "发布版本"
    echo 已删除发布目录
) else (
    echo 发布目录不存在，跳过清理
)

echo.
echo [3/8] 清理 bin 目录...
for /d %%i in (*) do (
    if exist "%%i\bin" (
        rmdir /s /q "%%i\bin"
        echo 已删除 %%i\bin
    )
)

echo.
echo [4/8] 清理 obj 目录...
for /d %%i in (*) do (
    if exist "%%i\obj" (
        rmdir /s /q "%%i\obj"
        echo 已删除 %%i\obj
    )
)

echo.
echo [5/8] 恢复 NuGet 包...
for %%f in (*.sln) do (
    echo 恢复解决方案: %%f
    dotnet restore "%%f" --verbosity minimal
    if %errorlevel% neq 0 (
        echo NuGet包恢复失败！
        pause
        exit /b 1
    )
    goto :restore_success
)
:restore_success
echo NuGet包恢复成功

echo.
echo [6/8] 构建解决方案...
for %%f in (*.sln) do (
    echo 构建解决方案: %%f
    dotnet build "%%f" --configuration Release --no-restore --verbosity minimal
    if %errorlevel% neq 0 (
        echo 构建失败！
        pause
        exit /b 1
    )
    goto :build_success
)
:build_success
echo 解决方案构建成功

echo.
echo [7/8] 发布主程序 RobotApp...
dotnet publish RobotApp/RobotApp.csproj --configuration Release --output "发布版本/PC28机器人" --self-contained false --verbosity minimal --no-restore
if %errorlevel% neq 0 (
    echo RobotApp 发布失败！
    pause
    exit /b 1
)
echo RobotApp 发布成功

echo.
echo [8/8] 发布辅助程序 DummyApp...
dotnet publish DummyApp/DummyApp.csproj --configuration Release --output "发布版本/PC28机器人/DummyApp" --self-contained false --verbosity minimal --no-restore
if %errorlevel% neq 0 (
    echo DummyApp 发布失败！
    pause
    exit /b 1
)
echo DummyApp 发布成功

echo.
echo ====================================
echo        打包完成！
echo ====================================
echo.
echo 发布文件位置:
echo   主程序: 发布版本\PC28机器人\
echo   辅助程序: 发布版本\PC28机器人\DummyApp\
echo.

echo 检查主程序文件:
for %%f in ("发布版本\PC28机器人\*.exe") do (
    echo   文件名: %%~nxf
    echo   大小: %%~zf 字节
    echo   修改时间: %%~tf
    echo 主程序文件创建成功！
    goto :check_done
)
echo 未找到主程序 exe 文件！

:check_done

echo.
echo 检查辅助程序文件:
for %%f in ("发布版本\PC28机器人\DummyApp\*.exe") do (
    echo   文件名: %%~nxf
    echo   大小: %%~zf 字节
    echo   修改时间: %%~tf
    echo 辅助程序文件创建成功！
    goto :dummy_check_done
)
echo 未找到辅助程序 exe 文件！

:dummy_check_done

REM 复制资源文件到Data目录
echo.
echo 复制资源文件到Data目录...

REM 创建Data目录
if not exist "发布版本\PC28机器人\Data" (
    mkdir "发布版本\PC28机器人\Data"
    echo 已创建Data目录
)

if exist "RobotApp\Configs" (
    xcopy /E /I /Y "RobotApp\Configs" "发布版本\PC28机器人\Data\Configs" >nul
    echo 已复制配置文件到Data\Configs
)
if exist "RobotApp\Document" (
    xcopy /E /I /Y "RobotApp\Document" "发布版本\PC28机器人\Data\Document" >nul
    echo 已复制文档文件到Data\Document
)
if exist "RobotApp\Sounds" (
    xcopy /E /I /Y "RobotApp\Sounds" "发布版本\PC28机器人\Data\Sounds" >nul
    echo 已复制声音文件到Data\Sounds
)
if exist "RobotApp\Template" (
    xcopy /E /I /Y "RobotApp\Template" "发布版本\PC28机器人\Data\Template" >nul
    echo 已复制模板文件到Data\Template
)
if exist "RobotApp\Native" (
    xcopy /E /I /Y "RobotApp\Native" "发布版本\PC28机器人\Data\Native" >nul
    echo 已复制本地库文件到Data\Native
)

REM 复制程序需要的配置文件到程序根目录（向后兼容）
if exist "RobotApp\Configs\config.ini" (
    copy /Y "RobotApp\Configs\config.ini" "发布版本\PC28机器人\" >nul
    echo 已复制主配置文件到程序根目录
)

echo.
echo 所有文件准备完成！可以运行程序进行测试。
echo.

REM 询问是否立即启动程序
set /p restart="是否立即启动PC28机器人程序？(Y/N): "
if /I "%restart%"=="Y" (
    echo.
    echo 正在启动PC28机器人程序...
    start "" "发布版本\PC28机器人\PC28机器人.exe"
    if %errorlevel% equ 0 (
        echo 程序启动成功！
    ) else (
        echo 程序启动失败，请手动运行。
    )
    echo.
    echo 按任意键退出...
    pause >nul
) else (
    echo.
    echo 打包完成！您可以手动运行程序进行测试。
    echo 程序位置: 发布版本\PC28机器人\PC28机器人.exe
    echo.
    echo 按任意键退出...
    pause >nul
)