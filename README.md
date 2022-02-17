# yopngs

示例站点：[有图床](https://yopngs.com)

[![Docker Image master](https://github.com/xpnas/yopngs/actions/workflows/docker-image.yml/badge.svg?branch=master)](https://github.com/xpnas/yopngs/actions/workflows/docker-image.yml)

一个纯粹的开源图床，聚焦图床核心功能，抛去用户验证、上传限制，自带鉴黄功能

支持鉴黄、支持压缩、支持本地存储、COS存储、OSS存储、B2存储

## 使用方法

### 发布版

  请先确认已安装DockerCompose
  ```
  wget "https://raw.githubusercontent.com/xpnas/yopngs/master/docker-compose.yml" 
  ```
  ```
  docker-compose up -d
   ```

### 配置Nginx代理
  ``` yml
  server
  {
   location / {
   proxy_pass http://localhost:8081;
   proxy_http_version 1.1;
   proxy_set_header Upgrade $http_upgrade;
   proxy_set_header Connection keep-alive;
   proxy_set_header Host $host;
   proxy_cache_bypass $http_upgrade;
    }
  }
  ```

## 配置存储源

所有配置都在config目录下的setting.json文件,可参照defaultsetting.json修改

### 本地存储
DISKStores节点，支持多个，可使用docker启动命令映射Rclone挂载的磁盘
``` json
  "DISKStores": [
    {
      "diskfloder": "/yopngs",//本地目录，docker请做映射
      "webfloder": "/v1",//url目录，如https://yopngs.com/v1/2022/01/01/xxxxx.png
      "name": "yopngs",//主界面下拉显示名称，随意填写
      "type": "yopngs",//内部类型，随意填写
      "index": 0,//主界面下拉排序，越小越优先
      "active": true//是否激活
    },
```
### Backblaze2存储
B2Stores节点，支持多个
```json
"B2Stores": [
  {
    "KeyId": "xx",
    "ApplicationKey": "xx",
    "BucketId": "xx",
    "Domain": "https://xx.com",//建议在B2前套上Cloudflare，使用自定义域名
    "Safe":false,//建议使用Cloudflare规则以避免暴露B2信息，防止有心人刷B2流量，开启后将去除Url中的file/BucketName
    "name": "backblazeb2",
    "type": "backblazeb2",
    "index": "2",
    "active": true
   }
  ```
### 腾讯COS存储
COSStores节点，支持多个
``` json
  "COSStores": [
    {
      "region": "ap-shanghai",
      "bucket": "xx",
      "SECRET_ID": "xx",
      "SECRET_KEY": "xx",
      "Domain": "https://xx.com",
      "name": "COS",
      "type": "COS",
      "index": 1,
      "active": false
    }
  ],
  ```
### 阿里OSS存储
OSSStores节点，支持多个
``` json
  "OSSStores": [
    {
      "AccessKeyId": "xxx",
      "AccessKeySecret": "xx",
      "Endpoint": "xx",
      "Domain": "https://xx.com",
      "name": "OSS",
      "type": "OSS",
      "index": "2",
      "active": false
    }
  ],
```
## 其他设置

```json
  "GLOBAL": {
    "SIZELIMIT": 30,//图片大小
    "EXTLIMIT": ".PNG.GIF.JPG.JPEG.BMP",//类型限制
    "NSFW": true,//鉴黄开关
    "NSFWCORE": 0.5,//鉴黄分数0~1
    "NSFWHOST": "http://nsfwapi:5000",//请勿修改
    "SERVERHOST": "http://yopngs:80",//请勿修改
    "COMPRESS": false,//是否启用压缩
    "COUNT": 0,
    "STARTDATE": "2020.01.01"
  },
```
