﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NGProjectAdmin.Entity.BusinessDTO.NGBusiness;
using NGProjectAdmin.Entity.BusinessDTO.SystemManagement;
using NGProjectAdmin.Entity.BusinessEntity.BusinessModule;
using NGProjectAdmin.Entity.BusinessEntity.NGBusiness;
using NGProjectAdmin.Entity.BusinessEntity.SystemManagement;
using NGProjectAdmin.Entity.BusinessEnum;
using NGProjectAdmin.Entity.CoreEntity;
using NGProjectAdmin.Service.BusinessService.NGBusiness;
using NGProjectAdmin.WebApi.AppCode.ActionFilters;
using NGProjectAdmin.WebApi.AppCode.FrameworkBase;
using System.Threading.Tasks;

namespace NGProjectAdmin.WebApi.Controllers.NGBusiness
{
    /// <summary>
    /// 资产档案
    /// </summary>
    public class AssetController : BaseController<Assets_info>
    {
        private readonly IAssets_infoService Assets_infoService;

        private readonly IAssetment_groupService Assetment_groupService;

        private readonly IAssetment_detailService Assetment_detailService;

        private readonly IContract_baseinfoService Contract_baseinfoService;

        private readonly IAssets_groupService Assets_groupService;

        private readonly IAssets_detailService Assets_detailService;

        /// <summary>
        /// AutoMapper实例
        /// </summary>
        private readonly IMapper mapper;

        /// <summary>
        /// 接口服务
        /// </summary>
        /// <param name="assets_infoService"></param>
        /// <param name="contract_baseinfoService"></param>
        /// <param name="assets_groupService"></param>
        /// <param name="assets_detailService"></param>
        /// <param name="assetment_groupService"></param>
        /// <param name="assetment_detailService"></param>
        /// <param name="mapper"></param>
        public AssetController(IAssets_infoService assets_infoService, IContract_baseinfoService contract_baseinfoService, IAssets_groupService assets_groupService, IAssets_detailService assets_detailService, IAssetment_groupService assetment_groupService, IAssetment_detailService assetment_detailService, IMapper mapper) : base(assets_infoService)
        {
            this.Assets_infoService = assets_infoService;
            this.Assetment_groupService = assetment_groupService;
            this.Assetment_detailService = assetment_detailService;
            this.Assets_groupService = assets_groupService;
            this.Assets_detailService = assets_detailService;
            this.Contract_baseinfoService = contract_baseinfoService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 查询资产档案列表
        /// </summary>
        /// <param name="queryCondition"></param>
        /// <returns></returns>
        [HttpPost]
        [Log(OperationType.QueryList)]
        [AllowAnonymous]
        //[Permission("Asset:qurey:list")]
        public async Task<IActionResult> Post(QueryCondition queryCondition)
        {
            var actionResult = await this.Assets_infoService.GetAssetInfoListAsync(queryCondition);
            return Ok(actionResult);

        }
        /// <summary>
        /// 新增资产档案
        /// </summary>
        /// <param name="assets_info">资产档案对象</param>
        /// <returns>ActionResult</returns>
        [HttpPost]
        [Log(OperationType.AddEntity)]
        [Permission("assetinfo:add:entity")]
        public async Task<IActionResult> Add([FromBody] Assets_infoDTO assets_info)
        {
            Assetment_group assetment_Group = new Assetment_group() { BuildDate = assets_info.assetsMent.buildDate };
            Assetment_detail assetment_detail = new Assetment_detail();

            //新建资产档案
            await this.Assets_infoService.AddAsync(assets_info);

            // 新建资产评估主表丶从表
            await Assetment_groupService.AddAsync(assetment_Group, true);
            assetment_detail.AssetMentId = assetment_Group.Id;
            assetment_detail.AssetId = assets_info.Id;
            assetment_detail.AssetAdress = assets_info.AssetsAdress;
            assetment_detail.measureArea = assets_info.assetsMent.assessArea;
            assetment_detail.AssessArea = assets_info.AssetsArea;
            assetment_detail.AssetPriceOneYear = assets_info.assetsMent.assetPriceOneYear;
            await Assetment_detailService.AddAsync(assetment_detail, true);


            //新建合同对应资产信息主表丶从表
            Assets_group assets_Group = new Assets_group();
            assets_Group.Area = assets_info.assetsMent.assessArea;
            await Assets_groupService.AddAsync(assets_Group, true);
            Assets_detail assets_Detail = new Assets_detail();
            assets_Detail.AssetAdress=assets_info.AssetsAdress;
            assets_Detail.AssetArea = assets_info.assetsMent.assessArea;
            assets_Detail.AssetsId=assets_info.Id;
            assets_Detail.insideId = 1;
            assets_Detail.GroupId = assets_Group.Id;
            await Assets_detailService.AddAsync(assets_Detail, true);

            //新建合同
            var contact = mapper.Map<Contract_baseinfo>(assets_info.contractinfo);
            if (contact == null)
            {
                contact=new Contract_baseinfo();
            }
            contact.AssetsId = assets_Group.Id;
            await Contract_baseinfoService.AddAsync(contact, true);

            //修改资产档案信息
            assets_info.AssetsMentGroupId = assetment_Group.Id;
            assets_info.ContractCode = contact.Id;
            var actionResult= await this.Assets_infoService.UpdateAsync(assets_info);


            return Ok(actionResult);
        }
    }
}
