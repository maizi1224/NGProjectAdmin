using NGProjectAdmin.Entity.Base;
using SqlSugar;
using System;
using System.ComponentModel.DataAnnotations;

namespace NGProjectAdmin.Entity.BusinessEntity.BusinessModule
{
    /// <summary>
    /// BizUserModule Entity Model
    /// </summary>   
    //[Serializable]
    [SugarTable("biz_user_module")]
    public class BizUserModule : NGAdminBaseEntity
    {
        /// <summary>
        /// 模块编号
        /// </summary>
        [Required]
        [SugarColumn(ColumnName = "MODULE_ID")]
        public Guid ModuleId { get; set; }

        /// <summary>
        /// 用户编号
        /// </summary>
        [Required]
        [SugarColumn(ColumnName = "USER_ID")]
        public Guid UserId { get; set; }

        /// <summary>
        /// 用户所在模块登录账号
        /// </summary>
        [Required]
        [MaxLength(128)]
        [SugarColumn(ColumnName = "USER_MODULE_LOGON_NAME")]
        public String UserModuleLogonName { get; set; }

        /// <summary>
        /// 用户所在模块登录密码
        /// </summary>
        [Required]
        [MaxLength(512)]
        [SugarColumn(ColumnName = "USER_MODULE_PASSWORD")]
        public String UserModulePassword { get; set; }
    }
}
