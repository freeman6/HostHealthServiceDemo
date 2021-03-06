# HostHealthServiceDemo

## 主要提供大家參考如何利用 PerformanceCounter API 來蒐集主機資訊，這個 Demo 幾項重點說明：
### 1. 利用 PerformanceCounter 收集 CPU、Memory、Disk IO 使用率。
### 2. 利用 GetDiskFreeSpace Win32 API 收集磁碟資訊。
### 3. 利用 Windows Service 收集相關資訊。
### 4. 利用 Line 及 EMail 發送異常通知(簡易版)。
### 5. 判斷 Web Service 服務維運狀況及<del>異常時自動重啟服務</del>。
　　- Application Pool 可使用 [Microsoft.Web.Administration](https://docs.microsoft.com/zh-tw/dotnet/api/microsoft.web.administration?redirectedfrom=MSDN&view=iis-dotnet) API 來偵測及重啟。
### 6. LineBot 換上董大偉老師的 [LineBotSDK](https://www.nuget.org/packages/LineBotSDK)，這樣子大家比較好用些。
  
## 預留項目，可自行補上
### 1. 收集指定的 Application Pool 所使用的 CPU、Memory ...
