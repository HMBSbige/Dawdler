# Dawdler
[![CI](https://github.com/HMBSbige/Dawdler/actions/workflows/CI.yml/badge.svg)](https://github.com/HMBSbige/Dawdler/actions/workflows/CI.yml)
[![Docker](https://github.com/HMBSbige/Dawdler/actions/workflows/Docker.yml/badge.svg)](https://github.com/HMBSbige/Dawdler/actions/workflows/Docker.yml)
[![Docker](https://img.shields.io/badge/Docker-dawdler-blue?label=Docker&logo=docker)](https://github.com/users/HMBSbige/packages/container/package/dawdler)
[![Github last commit date](https://img.shields.io/github/last-commit/HMBSbige/Dawdler.svg?label=Updated&logo=github)](https://github.com/HMBSbige/Dawdler/commits)

# 功能
* Bilibili
  * 漫画
    * 每日签到
    * 每日分享
  * 直播
    * 每日自动发送弹幕获取 100 亲密度
* 百度
  * 每日贴吧签到

# 配置
所有的配置文件均在 `configs/` 目录下，运行前应手动配置好

* `configs`
  * `bilibiliusers.json`
  * `baiduusers.json`

## bilibiliusers.json
B 站用户配置，支持多用户
```json
[
  {
    "Username": "用户名",
    "Password": "密码",
    "AccessToken": "",
    "RefreshToken": "",
    "Csrf": "",
    "Cookie": ""
  },
  {
    "Username": "",
    "Password": ""
  }
]
```

## baiduusers.json
百度用户配置，支持多用户

BUSS 可以从 Cookie 中自行获取

```json
[
  {
    "BDUSS": "BDUSS"
  }
]
```


# Usage
## Docker
### 拉取/更新最新镜像
```
docker pull ghcr.io/hmbsbige/dawdler
```
### 运行
```
docker run \
-it \
--name=dawdler \
-v /etc/localtime:/etc/localtime:ro \
-v $(pwd)/configs:/app/configs \
-v $(pwd)/Logs:/app/Logs \
ghcr.io/hmbsbige/dawdler
```

## 其他平台
在 [Actions](https://github.com/HMBSbige/Dawdler/actions/workflows/CI.yml?query=workflow%3ACI+branch%3Amaster+is%3Asuccess) 中选择最新的 Commit 下载所需平台的 Artifact
