using Iimages.IStore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Iimages.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private static object CountValue = "";

        private static object DateValue = "";

        private IHostEnvironment mEenvironment;



        public ApiController(IHostEnvironment hostEnvironment)
        {
            mEenvironment = hostEnvironment;
            CountValue = GetCountValue(false);
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadAsync(string type)
        {
            return await Task.Run(() =>
              {
                  //类型检测
                  IDataStore store;
                  if (!SotreCenter.Stores.TryGetValue(type, out store))
                  {
                      return Ok(new
                      {
                          UploadNode = type,
                          code = 404,
                          data = new
                          {
                              msg = "ERROR",
                              name = "ERROR",
                              url = "ERROR",
                          },
                          msg = string.Format("上传失败!不支持的类型{0}!", type)
                      });
                  }

                  //文件数量检测
                  var form = Request.Form;
                  var files = form.Files;
                  if (files.Count != 1)
                  {
                      return Ok(new
                      {
                          UploadNode = type,
                          code = 404,
                          data = new
                          {
                              msg = "ERROR",
                              name = "ERROR",
                              url = "ERROR",
                          },
                          msg = "上传失败!"
                      });
                  }

                  //文件大小检测
                  var formFile = files[0];
                  var sizeLimit = Convert.ToInt32(SotreCenter.Config["GLOBAL:SIZELIMIT"]);
                  if (formFile.Length < 0 || formFile.Length > sizeLimit * 1024 * 1024)
                  {
                      return Ok(new
                      {
                          UploadNode = type,
                          code = 404,
                          data = new
                          {
                              msg = "ERROR",
                              name = "ERROR",
                              url = "ERROR",
                          },
                          msg = string.Format("上传失败!限制大小{0}-{1}M", 0, sizeLimit)
                      });
                  }

                  //格式检测
                  var sourceName = formFile.FileName;
                  var fileExt = Path.GetExtension(sourceName);
                  var extLimit = SotreCenter.Config["GLOBAL:EXTLIMIT"];
                  if (!extLimit.Contains(fileExt.ToUpper()))
                  {
                      return Ok(new
                      {
                          UploadNode = type,
                          code = 404,
                          data = new
                          {
                              msg = "ERROR",
                              name = "ERROR",
                              url = "ERROR",
                          },
                          msg = string.Format("上传失败!限制格式{0}", 0, extLimit)
                      });
                  }

                  try
                  {

                      //黄图检测
                      var localName = Guid.NewGuid().ToString() + fileExt;
                      var webServer = Request.Scheme + "://" + Request.Host;
                      var nswFilePath = string.Empty;


                      using (var memoery = new MemoryStream())
                      {
                          memoery.SetLength(formFile.Length);
                          formFile.CopyTo(memoery);
                          var maps = memoery.GetBuffer();

                          //鉴黄
                          if (SotreCenter.NSFW && !SotreCenter.NSFWCHECK.PassSex(maps))
                          {
                              return Ok(new
                              {
                                  UploadNode = type,
                                  code = 404,
                                  data = new
                                  {
                                      msg = "ERROR",
                                      name = sourceName,
                                      url = "ERROR",
                                  },
                                  msg = "小撸怡情，大撸伤身!"
                              });
                          }

                          //压缩
                          if (SotreCenter.COMPRESS && fileExt.ToUpper() == ".PNG")
                          {
                              maps = SotreCenter.StoreCompress.Compress(maps);
                          }

                          //存储
                          var cdnUrl = string.Empty;
                          if (store.Up(maps, localName, webServer, ref cdnUrl))
                          {
                              CountValue = GetCountValue(true);

                              return Ok(new
                              {
                                  UploadNode = type,
                                  code = 200,
                                  data = new
                                  {
                                      msg = "OK",
                                      name = sourceName,
                                      url = cdnUrl,
                                  },
                                  msg = "上传成功!"
                              });
                          }

                          return Ok(new
                          {
                              UploadNode = type,
                              code = 502,
                              data = new
                              {
                                  msg = "ERROR",
                                  name = sourceName,
                                  url = "ERROR",
                              },
                              msg = "上传失败!"
                          });
                      }
                  }
                  catch
                  {
                      return Ok(new
                      {
                          UploadNode = type,
                          code = 502,
                          data = new
                          {
                              msg = "ERROR",
                              name = sourceName,
                              url = "ERROR",
                          },
                          msg = "上传失败!"
                      });

                  }
              });
        }

        [HttpGet]
        [Route("supports")]
        public List<SupportType> GetSupports()
        {
            return SotreCenter.Supports;
        }

        [HttpGet]
        [Route("info")]
        public string Info()
        {
            var startDate = DateTime.ParseExact(GetDateValue() as string, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
            var timeSpan = DateTime.Now - startDate;
            return string.Format("本站已运行 {0} 天,托管 {1} 张图片", timeSpan.Days, Convert.ToInt64(CountValue));

        }


        [HttpGet]
        public string Get()
        {
            return "It's Work";
        }

        private object GetCountValue(bool add)
        {
            lock (CountValue)
            {
                var filePath = Path.Combine(mEenvironment.ContentRootPath, "config/count.txt");
                if (!System.IO.File.Exists(filePath))
                    System.IO.File.WriteAllText(filePath, "0");

                CountValue = System.IO.File.ReadAllText(filePath);

                if (add)
                {
                    CountValue = (Convert.ToInt64(CountValue) + 1).ToString();
                    System.IO.File.WriteAllText(filePath, CountValue as string);
                }
            }

            return CountValue;
        }

        private object GetDateValue()
        {

            lock (DateValue)
            {
                if (string.IsNullOrEmpty(DateValue as string))
                {
                    var filePath = Path.Combine(mEenvironment.ContentRootPath, "config/date.txt");
                    if (!System.IO.File.Exists(filePath))
                        System.IO.File.WriteAllText(filePath, DateTime.Now.ToString("yyyyMMdd"));

                    DateValue = System.IO.File.ReadAllText(filePath);
                }
            }

            return DateValue;
        }
    }
}
