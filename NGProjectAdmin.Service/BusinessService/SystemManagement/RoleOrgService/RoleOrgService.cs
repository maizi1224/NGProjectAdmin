//-----------------------------------------------------------------------
// <Copyright file="SysRoleOrgService.cs" company="RuYiAdmin">
// * Copyright (C) 2021 RuYiAdmin All Rights Reserved
// * Version : 4.0.30319.42000
// * Author  : auto generated by RuYiAdmin T4 Template
// * FileName: SysRoleOrgService.cs
// * History : Created by RuYiAdmin 11/08/2021 10:23:17
// </Copyright>
//-----------------------------------------------------------------------

using NGProjectAdmin.Common.Global;
using NGProjectAdmin.Common.Utility;
using NGProjectAdmin.Entity.BusinessEntity.SystemManagement;
using NGProjectAdmin.Entity.CoreEntity;
using NGProjectAdmin.Repository.BusinessRepository.RedisRepository;
using NGProjectAdmin.Repository.BusinessRepository.SystemManagement.RoleOrgRepository;
using NGProjectAdmin.Service.Base;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NGProjectAdmin.Service.BusinessService.SystemManagement.RoleOrgService
{
    /// <summary>
    /// 角色机构业务层实现
    /// </summary>
    public class RoleOrgService : NGAdminBaseService<SysRoleOrg>, IRoleOrgService
    {
        #region 属性及构造函数

        /// <summary>
        /// 仓储实例
        /// </summary>
        private readonly IRoleOrgRepository roleOrgRepository;

        /// <summary>
        /// Redis仓储实例
        /// </summary>
        private readonly IRedisRepository redisRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="roleOrgRepository"></param>
        public RoleOrgService(IRoleOrgRepository roleOrgRepository,
                              IRedisRepository redisRepository) : base(roleOrgRepository)
        {
            this.roleOrgRepository = roleOrgRepository;
            this.redisRepository = redisRepository;
        }

        #endregion

        #region 服务层公有方法

        #region 加载角色与机构缓存

        /// <summary>
        /// 加载角色与机构缓存
        /// </summary>
        public async Task LoadSystemRoleOrgCache()
        {
            var sqlKey = "sqls:sql:query_role_org_info";
            var strSQL = NGAdminGlobalContext.Configuration.GetSection(sqlKey).Value;

            int totalCount = 0;
            var roleOrgs = (await this.roleOrgRepository.SqlQueryAsync<SysRoleOrg>(new QueryCondition(), totalCount, strSQL)).ToList();
            await this.redisRepository.SetAsync(NGAdminGlobalContext.SystemCacheConfig.RoleAndOrgCacheName, roleOrgs, -1);

            NGLoggerContext.Info("系统角色与机构缓存加载完成");
        }

        #endregion

        #region 清理角色与机构缓存

        /// <summary>
        /// 清理角色与机构缓存
        /// </summary>
        public async Task ClearSystemRoleOrgCache()
        {
            await this.redisRepository.DeleteAsync(new String[] { NGAdminGlobalContext.SystemCacheConfig.RoleAndOrgCacheName });

            NGLoggerContext.Info("系统角色与机构缓存清理完成");
        }

        #endregion

        #endregion
    }
}
