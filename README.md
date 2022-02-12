# yopngs

示例站点：[有图床](https://yopngs.com)

[![Docker Image master](https://github.com/xpnas/yopngs/actions/workflows/docker-image.yml/badge.svg)](https://github.com/xpnas/yopngs/actions/workflows/docker-image.yml)

一个纯粹的图床，聚焦图床核心功能，抛去用户验证、上传限制，自带鉴黄功能

## 使用方法

### 发布版（推荐）
  ```
docker run --name=yopngs -d -p 8081:80 -v /yopngs:/yopngs -v /yopngs_config:/app/config --restart=always xpnas/yopngs:latest
  ```

### 开发版
  ```
docker run --name=yopngs -d -p 8081:80 -v /yopngs:/yopngs -v /yopngs_config:/app/config --restart=always xpnas/yopngs:master
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

DISKStores节点，支持多个
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
### 腾讯COS存储
COSStores节点，支持多个
``` json
  "COSStores": [
    {
      "region": "ap-shanghai",
      "bucket": "xx-xx",
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
      "AccessKeyId": "ap-shanghai",
      "AccessKeySecret": "xx-xx",
      "Endpoint": "xx",
      "Domain": "https://xx.com",
      "name": "OSS",
      "type": "OSS",
      "index": "2",
      "active": false
    }
  ],
```
