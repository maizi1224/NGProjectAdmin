//-----------------------------------------------------------------------
// <Copyright file="BizUserModuleManagementController.cs" company="RuYiAdmin">
// * Copyright (C) 2022 RuYiAdmin All Rights Reserved
// * Version : 4.0.30319.42000
// * Author  : auto generated by RuYiAdmin T4 Template
// * FileName: BizUserModuleManagementController.cs
// * History : Created by RuYiAdmin 01/21/2022 13:22:06
// </Copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NGProjectAdmin.Common.Class.Exceptions;
using NGProjectAdmin.Common.Global;
using NGProjectAdmin.Common.Utility;
using NGProjectAdmin.Entity.BusinessDTO.BusinessModule;
using NGProjectAdmin.Entity.BusinessDTO.SystemManagement;
using NGProjectAdmin.Entity.BusinessEntity.BusinessModule;
using NGProjectAdmin.Entity.BusinessEnum;
using NGProjectAdmin.Entity.CoreEntity;
using NGProjectAdmin.Repository.Base;
using NGProjectAdmin.Service.BusinessService.BusinessModule.BusinessModuleService;
using NGProjectAdmin.Service.BusinessService.BusinessModule.BusinessUserModuleService;
using NGProjectAdmin.Service.BusinessService.BusinessModule.BusinessUserService;
using NGProjectAdmin.Service.BusinessService.RedisService;
using NGProjectAdmin.Service.BusinessService.SystemManagement.ImportService;
using NGProjectAdmin.WebApi.AppCode.ActionFilters;
using NGProjectAdmin.WebApi.AppCode.FrameworkBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NGProjectAdmin.WebApi.Controllers.BusinessModule
{
    /// <summary>
    /// BizUserModule控制器
    /// </summary>
    public class BizUserModuleManagementController : NGAdminBaseController<BizUserModule>
    {
        #region 属性及构造函数

        /// <summary>
        /// 模块用户接口实例
        /// </summary>
        private readonly IBizUserModuleService BizUserModuleService;

        /// <summary>
        /// 业务用户接口实例
        /// </summary>
        private readonly IBizUserService BizUserService;

        /// <summary>
        /// 业务模块接口实例
        /// </summary>
        private readonly IBizModuleService BizModuleService;

        /// <summary>
        /// 导入配置服务接口实例
        /// </summary>
        private readonly IImportConfigService ImportConfigService;

        /// <summary>
        /// Redis服务接口实例
        /// </summary>
        private readonly IRedisService RedisService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="BizUserModuleService"></param>
        /// <param name="BizUserService"></param>
        /// <param name="BizModuleService"></param>
        /// <param name="ImportConfigService"></param>
        /// <param name="RedisService"></param>
        public BizUserModuleManagementController(IBizUserModuleService BizUserModuleService,
                                                 IBizUserService BizUserService,
                                                 IBizModuleService BizModuleService,
                                                 IImportConfigService ImportConfigService,
                                                 IRedisService RedisService) : base(BizUserModuleService)
        {
            this.BizUserModuleService = BizUserModuleService;
            this.BizUserService = BizUserService;
            this.BizModuleService = BizModuleService;
            this.ImportConfigService = ImportConfigService;
            this.RedisService = RedisService;
        }

        #endregion

        #region 查询模块用户列表

        /// <summary>
        /// 查询模块用户列表
        /// </summary>
        /// <param name="queryCondition">查询条件</param>
        /// <returns>ActionResult</returns>
        [HttpPost]
        [Log(OperationType.QueryList)]
        [Permission("user:module:list")]
        public async Task<IActionResult> Post(QueryCondition queryCondition)
        {
            var actionResult = await this.BizUserModuleService.SqlQueryAsync<BizUserModuleDTO>(queryCondition, "sqls:sql:query_biz_user_module_dto");
            return Ok(actionResult);
        }

        #endregion

        #region 查询模块用户信息

        /// <summary>
        /// 查询模块用户信息
        /// </summary>
        /// <param name="id">编号</param>
        /// <returns>ActionResult</returns>
        [HttpGet("{id}")]
        [Log(OperationType.QueryEntity)]
        [Permission("user:module:list")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var list = (await this.BizUserModuleService.SqlQueryAsync<BizUserModuleDTO>(new QueryCondition(), "sqls:sql:query_biz_user_module_dto")).List;

            var actionResult = new Entity.CoreEntity.ActionResult();
            actionResult.HttpStatusCode = HttpStatusCode.OK;
            actionResult.Message = new String("OK");
            actionResult.Object = list.Where(t => t.Id == id).FirstOrDefault();

            return Ok(actionResult);
        }

        #endregion

        #region 新增业务模块用户

        /// <summary>
        /// 新增业务模块用户
        /// </summary>
        /// <param name="bizUserModule">模块用户对象</param>
        /// <returns>ActionResult</returns>
        [HttpPost]
        [Log(OperationType.AddEntity)]
        [Permission("user:module:add")]
        public async Task<IActionResult> Add([FromBody] BizUserModuleDTO bizUserModule)
        {
            var module = (BizModule)(await this.BizModuleService.GetByIdAsync(bizUserModule.ModuleId)).Object;

            var user = await this.BizUserService.GetBizUser(module.ModuleShortNameEN, bizUserModule.UserLogonName);
            if (user != null)
            {
                throw new NGAdminCustomException("logon name has existed");
            }

            var defaultPassword = NGAdminGlobalContext.SystemConfig.DefaultPassword;
            var aesKey = NGAdminGlobalContext.SystemConfig.AesKey;

            //新增业务用户
            user = new BizUser();
            user.UserLogonName = String.Join('_', module.ModuleShortNameEN, bizUserModule.UserLogonName);
            user.UserDisplayName = bizUserModule.UserDisplayName;
            user.Salt = Guid.NewGuid();
            user.UserPassword = NGAesUtil.Encrypt(defaultPassword + user.Salt, aesKey);
            user.Telephone = bizUserModule.Telephone;
            user.MobilePhone = bizUserModule.MobilePhone;
            user.Email = bizUserModule.Email;
            user.Sex = bizUserModule.Sex;
            user.IsEnabled = bizUserModule.IsEnabled;
            await this.BizUserService.AddAsync(user);

            //新增模块与用户关系
            var userModule = new BizUserModule();
            userModule.UserId = user.Id;
            userModule.ModuleId = bizUserModule.ModuleId;
            userModule.UserModuleLogonName = bizUserModule.UserModuleLogonName;
            userModule.UserModulePassword = NGRsaUtil.PemDecrypt(bizUserModule.UserModulePassword, NGAdminGlobalContext.SystemConfig.RsaPrivateKey);
            var actionResult = await this.BizUserModuleService.AddAsync(userModule);

            return Ok(actionResult);
        }

        #endregion

        #region 编辑业务模块用户

        /// <summary>
        /// 编辑业务模块用户
        /// </summary>
        /// <param name="bizUserModule">模块用户对象</param>
        /// <returns>ActionResult</returns>
        [HttpPut]
        [Log(OperationType.EditEntity)]
        [Permission("user:module:edit")]
        public async Task<IActionResult> Put([FromBody] BizUserModuleDTO bizUserModule)
        {
            var module = (BizModule)(await this.BizModuleService.GetByIdAsync(bizUserModule.ModuleId)).Object;
            var user = (BizUser)(await this.BizUserService.GetByIdAsync(bizUserModule.UserId)).Object;

            if (user != null)
            {
                user.UserDisplayName = bizUserModule.UserDisplayName;

                var password = NGRsaUtil.PemDecrypt(bizUserModule.UserPassword, NGAdminGlobalContext.SystemConfig.RsaPrivateKey);
                if (password != user.UserPassword)
                {
                    var defaultPassword = NGAdminGlobalContext.SystemConfig.DefaultPassword;
                    var aesKey = NGAdminGlobalContext.SystemConfig.AesKey;
                    user.Salt = Guid.NewGuid();
                    user.UserPassword = NGAesUtil.Encrypt(defaultPassword + user.Salt, aesKey);
                }

                user.Telephone = bizUserModule.Telephone;
                user.MobilePhone = bizUserModule.MobilePhone;
                user.Email = bizUserModule.Email;
                user.Sex = bizUserModule.Sex;
                user.IsEnabled = bizUserModule.IsEnabled;
                var actionResult = await this.BizUserService.UpdateAsync(user);

                var userModule = await this.BizUserModuleService.GetBizUserModule(user.Id, module.Id);
                if (userModule != null)
                {
                    userModule.UserModuleLogonName = bizUserModule.UserModuleLogonName;
                    userModule.UserModulePassword = NGRsaUtil.PemDecrypt(bizUserModule.UserModulePassword, NGAdminGlobalContext.SystemConfig.RsaPrivateKey);
                    actionResult = await this.BizUserModuleService.UpdateAsync(userModule);
                }

                return Ok(actionResult);
            }

            throw new NGAdminCustomException("not found");
        }

        #endregion

        #region 删除模块用户关系

        /// <summary>
        /// 删除模块用户关系
        /// </summary>
        /// <param name="id">模块编号</param>
        /// <returns>ActionResult</returns>
        [HttpDelete("{id}")]
        [Permission("user:module:del")]
        [Log(OperationType.RemoveUnifiedAuthorization)]
        public async Task<IActionResult> Delete(Guid id)
        {
            //删除授权关系
            var actionResult = await this.BizUserModuleService.DeleteAsync(id);

            //var list = (List<BizUserModule>)this.BizUserModuleService.GetList().Object;
            //var serModule = list.Where(t => t.Id == id).FirstOrDefault();
            //var userId = serModule.UserId;
            //if (list.Where(t => t.UserId == userId && t.IsDel == 0).Count() <= 0)
            //{
            //    //删除业务用户
            //    this.BizUserService.Delete(userId);
            //}

            return Ok(actionResult);
        }

        #endregion

        #region 批量删除用户关系

        /// <summary>
        /// 批量删除用户关系
        /// </summary>
        /// <param name="ids">编号数组</param>
        /// <returns>ActionResult</returns>
        [HttpDelete("{ids}")]
        [Log(OperationType.RemoveUnifiedAuthorization)]
        [Permission("user:module:del")]
        public async Task<IActionResult> DeleteRange(String ids)
        {
            var array = NGStringUtil.GetGuids(ids);
            //删除授权关系
            var actionResult = await this.BizUserModuleService.DeleteRangeAsync(array);

            //var list = (List<BizUserModule>)this.BizUserModuleService.GetList().Object;
            //foreach (var item in array)
            //{
            //    var serModule = list.Where(t => t.Id == item).FirstOrDefault();
            //    var userId = serModule.UserId;
            //    if (list.Where(t => t.UserId == userId && t.IsDel == 0).Count() <= 0)
            //    {
            //        //删除业务用户
            //        this.BizUserService.Delete(userId);
            //    }
            //}

            return Ok(actionResult);
        }

        #endregion

        #region 统一认证用户登录

        /// <summary>
        /// 统一认证用户登录
        /// </summary>
        /// <param name="loginDTO">登录信息</param>
        /// <returns>ActionResult</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Logon([FromBody] LoginDTO loginDTO)
        {
            var actionResult = await this.BizUserModuleService.Logon(loginDTO);
            return Ok(actionResult);
        }

        #endregion

        #region 导入业务用户信息

        /// <summary>
        /// 导入业务用户信息
        /// </summary>
        /// <param name="file">excel文件</param>
        /// <param name="moduleId">moduleId</param>
        /// <returns>ActionResult</returns>
        [HttpPost]
        [Log(OperationType.ImportData)]
        [Permission("user:module:import")]
        public async Task<IActionResult> Import([FromForm] IFormFile file, Guid moduleId)
        {
            var actionResult = new Entity.CoreEntity.ActionResult();

            if (file != null)
            {
                #region 常规合法性校验

                //获取文件拓展名
                var extension = Path.GetExtension(file.FileName);
                //文件保存路径
                var filePath = NGFileContext.SaveFormFile(file);

                var configDTO = this.ImportConfigService.GetImportConfig("BizUserImportConfig");
                if (configDTO == null)
                {
                    throw new NGAdminCustomException("can not find import config");
                }

                configDTO.ExcelPath = filePath;
                //常规合法性校验
                var errorCount = configDTO.ValidationDetecting();

                #endregion

                if (errorCount > 0)
                {
                    #region 常规校验出不合规项

                    actionResult.Object = errorCount;
                    actionResult.Message = configDTO.ExcelPath.Replace(extension, "").Split('/')[1];

                    #endregion
                }
                else
                {
                    #region 常规业务性校验

                    var module = (BizModule)this.BizModuleService.GetById(moduleId).Object;

                    var xlxStream = new FileStream(configDTO.ExcelPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    var workbook = new HSSFWorkbook(xlxStream);
                    var worksheet = workbook.GetSheetAt(0);

                    for (var i = configDTO.StartRow; i <= worksheet.LastRowNum; i++)
                    {
                        var value = worksheet.GetRow(i).GetCell(2).GetCellValue().Trim();
                        if (!String.IsNullOrEmpty(value))
                        {
                            var user = await this.BizUserService.GetBizUser(module.ModuleShortNameEN, value);
                            if (user != null)
                            {
                                errorCount++;
                                worksheet.SetCellComment(i, 2, "用户已存在！");
                            }
                        }
                    }

                    #endregion

                    if (errorCount > 0)
                    {
                        #region 业务校验出不合规项

                        var xlxPath = configDTO.ExcelPath;
                        configDTO.ExcelPath = workbook.SaveAsXlx(xlxPath);

                        xlxStream.Close();
                        System.IO.File.Delete(xlxPath);

                        actionResult.Object = errorCount;
                        actionResult.Message = configDTO.ExcelPath.Replace(extension, "").Split('/')[1];

                        #endregion
                    }
                    else
                    {
                        #region 执行业务导入

                        var defaultPassword = NGAdminGlobalContext.SystemConfig.DefaultPassword;
                        var aesKey = NGAdminGlobalContext.SystemConfig.AesKey;

                        for (var i = configDTO.StartRow; i <= worksheet.LastRowNum; i++)
                        {
                            var userName = worksheet.GetRow(i).GetCell(1).GetCellValue().Trim();
                            var userLogonName = worksheet.GetRow(i).GetCell(2).GetCellValue().Trim();
                            var userSex = worksheet.GetRow(i).GetCell(3).GetCellValue().Trim();
                            var modulePassword = worksheet.GetRow(i).GetCell(4).GetCellValue().Trim();
                            var mobilePhone = worksheet.GetRow(i).GetCell(5).GetCellValue().Trim();
                            var telephone = worksheet.GetRow(i).GetCell(6).GetCellValue().Trim();
                            var email = worksheet.GetRow(i).GetCell(7).GetCellValue().Trim();

                            var result = await this.BizUserService.UseTransactionAsync(async () =>
                              {
                                  //新增业务用户
                                  var user = new BizUser();
                                  user.UserLogonName = String.Join('_', module.ModuleShortNameEN, userLogonName);
                                  user.UserDisplayName = userName;
                                  user.Salt = Guid.NewGuid();
                                  user.UserPassword = NGAesUtil.Encrypt(defaultPassword + user.Salt, aesKey);
                                  user.Telephone = telephone;
                                  user.MobilePhone = mobilePhone;
                                  user.Email = email;
                                  switch (userSex)
                                  {
                                      case "男":
                                          user.Sex = 0;
                                          break;
                                      case "女":
                                          user.Sex = 1;
                                          break;
                                      case "第三性别":
                                          user.Sex = 2;
                                          break;
                                  }
                                  user.IsEnabled = (int)YesNo.YES;
                                  await this.BizUserService.AddAsync(user);

                                  //新增模块与用户关系
                                  var userModule = new BizUserModule();
                                  userModule.UserId = user.Id;
                                  userModule.ModuleId = moduleId;
                                  userModule.UserModuleLogonName = userLogonName;
                                  userModule.UserModulePassword = modulePassword;
                                  await this.BizUserModuleService.AddAsync(userModule);
                              });
                        }

                        workbook.Close();
                        xlxStream.Close();

                        #endregion
                    }
                }

                actionResult.HttpStatusCode = HttpStatusCode.OK;
            }
            else
            {
                actionResult.HttpStatusCode = HttpStatusCode.NoContent;
                actionResult.Message = new String("NoContent");
            }

            return Ok(actionResult);
        }

        #endregion

        #region 一对多访问授权

        /// <summary>
        /// 一对多访问授权
        /// </summary>
        /// <param name="bizUserModule">模块用户对象</param>
        /// <returns>ActionResult</returns>
        [HttpPost]
        [Log(OperationType.UnifiedAuthorization)]
        [Permission("user:module:grant,user:nonmodule:grant")]
        public async Task<IActionResult> Grant([FromBody] BizUserModule bizUserModule)
        {
            var list = (List<BizUserModule>)(await this.BizUserModuleService.GetListAsync()).Object;
            var count = list.Where(t => t.UserId == bizUserModule.UserId && t.ModuleId == bizUserModule.ModuleId && t.IsDel == 0).Count();

            if (count <= 0)
            {
                //新增模块与用户关系
                var userModule = new BizUserModule();
                userModule.UserId = bizUserModule.UserId;
                userModule.ModuleId = bizUserModule.ModuleId;
                userModule.UserModuleLogonName = bizUserModule.UserModuleLogonName;
                userModule.UserModulePassword = NGRsaUtil.PemDecrypt(bizUserModule.UserModulePassword, NGAdminGlobalContext.SystemConfig.RsaPrivateKey);

                var actionResult = await this.BizUserModuleService.AddAsync(userModule);
                return Ok(actionResult);
            }

            throw new NGAdminCustomException("relationship exited");
        }
        #endregion

        #region 禁用业务模块用户

        /// <summary>
        /// 禁用业务模块用户
        /// </summary>
        /// <param name="id">编号</param>
        /// <returns>ActionResult</returns>
        [HttpGet("{id}")]
        [Log(OperationType.EditEntity)]
        [Permission("user:nonmodule:grant")]
        public async Task<IActionResult> DisableUser(Guid id)
        {
            BizUser user = (await this.BizUserService.GetByIdAsync(id)).Object as BizUser;
            user.IsEnabled = (int)YesNo.NO;
            await this.BizUserService.UpdateAsync(user);
            return Ok(Entity.CoreEntity.ActionResult.OK());
        }

        #endregion

        #region 删除业务模块用户

        /// <summary>
        /// 删除业务模块用户
        /// </summary>
        /// <param name="ids">用户编号组</param>
        /// <returns>ActionResult</returns>
        [HttpDelete("{ids}")]
        [Permission("user:nonmodule:grant")]
        [Log(OperationType.DeleteEntity)]
        public async Task<IActionResult> DeleteUser(String ids)
        {
            var array = NGStringUtil.GetGuids(ids);
            await this.BizUserService.DeleteRangeAsync(array);//删除业务模块用户
            return Ok(Entity.CoreEntity.ActionResult.OK());
        }

        #endregion

        #region 修改业务用户密码

        /// <summary>
        /// 修改业务用户密码
        /// </summary>
        /// <param name="data">匿名对象</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UpdatePassword([FromBody] dynamic data)
        {
            var prikey = NGAdminGlobalContext.SystemConfig.RsaPrivateKey;
            var obj = JsonConvert.DeserializeObject(data.ToString());

            var token = NGRsaUtil.PemDecrypt(obj.token.Value, prikey);
            var userPassword = NGRsaUtil.PemDecrypt(obj.userPassword.Value, prikey);

            var arr = userPassword.ToString().Split('_');
            var userId = arr[1];
            //密码去盐
            userPassword = arr[0];

            var key = NGAdminGlobalContext.RedisConfig.UnifiedAuthenticationPattern + userId + "_" + token;
            var pattern = $"{key}*";
            var keys = this.RedisService.PatternSearch(pattern);
            if (keys.Count > 0)
            {
                var user = (BizUser)(await this.BizUserService.GetByIdAsync(Guid.Parse(userId))).Object;
                if (user != null)
                {
                    //AES加密
                    var aesKey = NGAdminGlobalContext.SystemConfig.AesKey;

                    user.Salt = Guid.NewGuid();
                    userPassword = NGAesUtil.Encrypt(userPassword + user.Salt, aesKey);
                    user.UserPassword = userPassword;
                    user.ModifyTime = DateTime.Now;
                    user.Modifier = user.Id;
                    user.VersionId = Guid.NewGuid();

                    await NGAdminDbScope.NGDbContext.Updateable<BizUser>(user).ExecuteCommandAsync();

                    return Ok();
                }
                else
                {
                    throw new NGAdminCustomException("user is not exited");
                }
            }
            else
            {
                throw new NGAdminCustomException("token is not exited");
            }
        }

        #endregion
    }
}
