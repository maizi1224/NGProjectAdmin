﻿using AutoMapper;
using Consul;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NGProjectAdmin.Common.Utility;
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
using SqlSugar;
using System;
using System.Collections.Generic;
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

        private readonly IFile_detailService File_detailService;

        private readonly Icontract_groupService Contract_groupService;

        /// <summary>
        /// AutoMapper实例
        /// </summary>
        private readonly IMapper mapper;

        /// <summary>
        /// 接口服务
        /// </summary>
        /// <param name="contract_groupService"></param>
        /// <param name="file_detailService"></param>
        /// <param name="assets_infoService"></param>
        /// <param name="contract_baseinfoService"></param>
        /// <param name="assets_groupService"></param>
        /// <param name="assets_detailService"></param>
        /// <param name="assetment_groupService"></param>
        /// <param name="assetment_detailService"></param>
        /// <param name="mapper"></param>
        public AssetController(Icontract_groupService contract_groupService, IFile_detailService file_detailService, IAssets_infoService assets_infoService, IContract_baseinfoService contract_baseinfoService, IAssets_groupService assets_groupService, IAssets_detailService assets_detailService, IAssetment_groupService assetment_groupService, IAssetment_detailService assetment_detailService, IMapper mapper) : base(assets_infoService)
        {
            this.Assets_infoService = assets_infoService;
            this.Assetment_groupService = assetment_groupService;
            this.Assetment_detailService = assetment_detailService;
            this.Assets_groupService = assets_groupService;
            this.Assets_detailService = assets_detailService;
            this.Contract_baseinfoService = contract_baseinfoService;
            this.File_detailService = file_detailService;
            this.Contract_groupService = contract_groupService;
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
        //[Permission("assetinfo:add:entity")]
        [AllowAnonymous]
        public async Task<IActionResult> Add([FromBody] Assets_infoDTO assets_info)
        {
            if (assets_info == null) { return ValidationProblem("资产信息不能为空"); }
            NGLoggerContext.Info(assets_info.ToJson());
            foreach (var item in assets_info.contractinfoByGroup)
            {
                if (item.contractinfoGroups != null)
                {
                    foreach (var value in item.contractinfoGroups)
                    {
                        assets_info.contractinfo.Add(value);
                    }
                }
            }
            Assetment_group assetment_Group = new Assetment_group() { BuildDate = assets_info.assetsMent.buildDate, AssetCode = assets_info.assetsMent.assetCode };
            Assetment_detail assetment_detail = new Assetment_detail();
            if (assets_info.assetDate != null)
            {
                assets_info.bgtime = assets_info.assetDate[0];
                assets_info.endtime = assets_info.assetDate[1];
            }
            //新建资产档案
            await this.Assets_infoService.AddAsync(assets_info);

            // 新建资产评估主表丶从表
            await Assetment_groupService.AddAsync(assetment_Group, true);
            assetment_detail.AssetMentId = assetment_Group.Id;
            assetment_detail.AssetId = assets_info.Id;
            assetment_detail.AssetAdress = assets_info.AssetsAdress;
            assetment_detail.measureArea = assets_info.assetsMent.assessArea;
            assetment_detail.AssessArea = assets_info.assetsMent.assessArea;
            assetment_detail.AssetPriceOneYear = assets_info.assetsMent.assetPriceOneYear;
            await Assetment_detailService.AddAsync(assetment_detail, true);


            //新建合同对应资产信息主表丶从表
            Assets_group assets_Group = new Assets_group();
            assets_Group.Area = assets_info.assetsMent.assessArea;
            await Assets_groupService.AddAsync(assets_Group, true);
            Assets_detail assets_Detail = new Assets_detail();
            assets_Detail.AssetAdress = assets_info.AssetsAdress;
            assets_Detail.AssetArea = assets_info.assetsMent.assessArea;
            assets_Detail.AssetsId = assets_info.Id;
            assets_Detail.insideId = 1;
            assets_Detail.GroupId = assets_Group.Id;
            await Assets_detailService.AddAsync(assets_Detail, true);

            //新建合同
            contract_group contract_Group = new contract_group();
            await Contract_groupService.AddAsync(contract_Group, true);
            if (assets_info.contractinfo != null)
            {
                foreach (Assets_info_ContractDTO item in assets_info.contractinfo)
                {

                    var contact = mapper.Map<Contract_baseinfo>(item);
                    if (contact == null)
                    {
                        contact = new Contract_baseinfo();
                    }
                    if (contact.ContractPrice > 0)
                    {
                        assets_info.AssetsState = 1;
                    }
                    contact.contract_groupId = contract_Group.Id;
                    contact.AssetsId = assets_info.Id;
                    await Contract_baseinfoService.AddAsync(contact, true);
                    await Contract_baseinfoService.BuildContractFeeInfo(contact);
                }
            }


            //修改资产档案信息
            assets_info.AssetsMentGroupId = assetment_Group.Id;
            assets_info.contract_groupId = contract_Group.Id;
            var actionResult = await this.Assets_infoService.UpdateAsync(assets_info);

            if (assets_info.contractinfo != null)
            {
                foreach (var item in assets_info.contractinfo)
                {
                    foreach (var k in item.contractPdfGroupFiles)
                    {
                        File_detail file_Detail = File_detailService.GetById(k.Id).Object as File_detail;
                        file_Detail.FileId = item.ContractPdfGroupId;
                        await File_detailService.UpdateAsync(file_Detail);
                    }
                }
            }


            if (assets_info.assetsFileGroupFiles != null)
            {
                foreach (var item in assets_info.assetsFileGroupFiles)
                {
                    File_detail file_Detail = File_detailService.GetById(item.Id).Object as File_detail;
                    if (file_Detail != null)
                    {
                        file_Detail.FileId = assets_info.AssetsFileGroupId;
                        await File_detailService.UpdateAsync(file_Detail);
                    }

                }
            }

            if (assets_info.propertyFileGroupFiles != null)
            {
                foreach (var item in assets_info.propertyFileGroupFiles)
                {
                    File_detail file_Detail = File_detailService.GetById(item.Id).Object as File_detail;
                    file_Detail.FileId = assets_info.propertyFileGroupId;
                    await File_detailService.UpdateAsync(file_Detail);
                }
            }


            return Ok(actionResult);
        }

        /// <summary>
        /// 查询资产信息
        /// </summary>
        /// <param name="assets_infoDTO">资产编号</param>
        /// <returns>ActionResult</returns>
        [HttpPost]
        [Log(OperationType.QueryEntity)]
        [Permission("asset:edit:entity")]
        public async Task<IActionResult> GetById([FromBody] Assets_infoDTO assets_infoDTO)
        {
            var actionResult = await this.Assets_infoService.GetAssetByIdAsync(assets_infoDTO);
            Assets_infoDTO asset = actionResult.Object as Assets_infoDTO;
            if (asset.AssetsFileGroupId != null)
            {
                var querynResult = await File_detailService.GetListAsync(new QueryCondition() { QueryItems = new List<QueryItem>() { new QueryItem() { DataType = NGProjectAdmin.Entity.CoreEnum.DataType.String, Field = "FileId", Value = asset.AssetsFileGroupId } } });
                asset.assetsFileGroupFiles = querynResult.List;
            }
            if (asset.propertyFileGroupId != null)
            {
                var querynResult2 = await File_detailService.GetListAsync(new QueryCondition() { QueryItems = new List<QueryItem>() { new QueryItem() { DataType = NGProjectAdmin.Entity.CoreEnum.DataType.String, Field = "FileId", Value = asset.propertyFileGroupId } } });
                asset.propertyFileGroupFiles = querynResult2.List;
            }
            List<Assets_info_ContractDTOGroup> assetGroups = new List<Assets_info_ContractDTOGroup>();
            if (asset.contractinfo.Count > 0)
            {                              
                foreach (var item in asset.contractinfo)
                {
                    if (!string.IsNullOrWhiteSpace(item.ContractPdfGroupId))
                    {
                        var querynResult3 = await File_detailService.GetListAsync(new QueryCondition() { QueryItems = new List<QueryItem>() { new QueryItem() { DataType = NGProjectAdmin.Entity.CoreEnum.DataType.String, Field = "FileId", Value = item.ContractPdfGroupId } } });
                        item.contractPdfGroupFiles = querynResult3.List;
                    }
                    // 按年度给合同分个组
                    DateTime dateTime = Convert.ToDateTime(item.contractDate);
                    Assets_info_ContractDTOGroup assets_Info_ContractDTOGroup = assetGroups.Find(x => x.year == dateTime.Year.ToString());
                    if (assets_Info_ContractDTOGroup != null)
                    {
                        if (assets_Info_ContractDTOGroup.contractinfoGroups == null)
                        {
                            assets_Info_ContractDTOGroup.contractinfoGroups = new List<Assets_info_ContractDTO>() { item };
                        }
                        else
                        {
                            assets_Info_ContractDTOGroup.contractinfoGroups.Add(item);
                        }
                    }
                    else
                    {
                        assets_Info_ContractDTOGroup = new Assets_info_ContractDTOGroup();
                        assets_Info_ContractDTOGroup.year = dateTime.Year.ToString();
                        assets_Info_ContractDTOGroup.contractinfoGroups = new List<Assets_info_ContractDTO>() { item };
                        assetGroups.Add(assets_Info_ContractDTOGroup);
                    }
                }                
            }
            asset.contractinfoByGroup = assetGroups;

            return Ok(actionResult);
        }
        /// <summary>
        /// 更新资产信息
        /// </summary>
        /// <param name="assets_info">资产信息</param>
        /// <returns>ActionResult</returns>
        [HttpPut]
        [Log(OperationType.QueryEntity)]
        [Permission("asset:edit:entity")]
        public async Task<IActionResult> UpdateById([FromBody] Assets_infoDTO assets_info )
        {
            try
            {
               
                var actionResult = await this.Assets_infoService.UpdateAsync(assets_info);
                foreach (var item in assets_info.contractinfoByGroup)
                {
                    if (item.contractinfoGroups != null)
                    {
                        foreach (var value in item.contractinfoGroups)
                        {
                            assets_info.contractinfo.Add(value);
                        }
                    }
                }


                foreach (Assets_info_ContractDTO item in assets_info.contractinfo)
                {
                    var contact = mapper.Map<Contract_baseinfo>(item);
                    if (contact != null && !string.IsNullOrWhiteSpace(contact.Id))
                    {
                        await this.Contract_baseinfoService.UpdateAsync(contact);
                    }
                    else
                    {
                        contact.contract_groupId = assets_info.contract_groupId;
                        contact.AssetsId = assets_info.Id;
                        await this.Contract_baseinfoService.AddAsync(contact, true);
                    }

                    if (contact.ContractPrice > 0)
                    {
                        assets_info.AssetsState = 1;
                    }
                    await Contract_baseinfoService.BuildContractFeeInfo(contact);
                }
                var actionResult2 = await Assetment_groupService.GetByIdAsync(assets_info.assetsMent.AssetMentId);
                Assetment_group assetment_Group = actionResult2.Object as Assetment_group;
                assetment_Group.BuildDate = assets_info.assetsMent.buildDate;
                assetment_Group.AssetCode = assets_info.assetsMent.assetCode;
                await Assetment_groupService.UpdateAsync(assetment_Group);

                var actionResult3 = await Assetment_detailService.GetByIdAsync(assets_info.assetsMent.id);
                Assetment_detail assetment_Detail = actionResult3.Object as Assetment_detail;
                assetment_Detail.AssessArea = assets_info.assetsMent.assessArea;
                assetment_Detail.measureArea = assets_info.assetsMent.assessArea;
                assetment_Detail.AssetPriceOneYear = assets_info.assetsMent.assetPriceOneYear;

                await Assetment_detailService.UpdateAsync(assetment_Detail);

                if (assets_info.assetsFileGroupFiles != null)
                {
                    foreach (var item in assets_info.assetsFileGroupFiles)
                    {
                        File_detail file_Detail = File_detailService.GetById(item.Id).Object as File_detail;
                        file_Detail.FileId = assets_info.AssetsFileGroupId;
                        await File_detailService.UpdateAsync(file_Detail);
                    }
                }

                if (assets_info.contractinfo != null)
                {
                    foreach (var item in assets_info.contractinfo)
                    {
                        if (item.contractPdfGroupFiles != null)
                        {
                            foreach (var k in item.contractPdfGroupFiles)
                            {
                                File_detail file_Detail = File_detailService.GetById(k.Id).Object as File_detail;
                                file_Detail.FileId = item.ContractPdfGroupId;
                                await File_detailService.UpdateAsync(file_Detail);
                            }
                        }

                    }
                }
                if (assets_info.propertyFileGroupFiles != null)
                {
                    foreach (var item in assets_info.propertyFileGroupFiles)
                    {
                        File_detail file_Detail = File_detailService.GetById(item.Id).Object as File_detail;
                        file_Detail.FileId = assets_info.propertyFileGroupId;
                        await File_detailService.UpdateAsync(file_Detail);
                    }
                }

                actionResult = await this.Assets_infoService.UpdateAsync(assets_info);
                return Ok(actionResult);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// 删除资产信息
        /// </summary>
        /// <param name="assets_infoDTO">资产编号</param>
        /// <returns>ActionResult</returns>
        [HttpPost]
        [Log(OperationType.DeleteEntity)]
        [Permission("asset:delete:entity")]
        public async Task<IActionResult> DeleteById([FromBody] Assets_infoDTO assets_infoDTO)
        {
            var actionResult = await Assets_infoService.DeleteAssetAndContract(assets_infoDTO);
            return Ok(actionResult);

        }


        /// <summary>
        /// 查询资产信息报表统计
        /// </summary>
        /// <returns>ActionResult</returns>
        [HttpPost]
        [AllowAnonymous]
        [Log(OperationType.QueryEntity)]
        public async Task<IActionResult> GetAssetsData()
        {
            var actionResult = await this.Assets_infoService.GetAssetsData();
            return Ok(actionResult);
        }


        /// <summary>
        /// 查询资产信息价值分类
        /// </summary>
        /// <returns>ActionResult</returns>
        [HttpPost]
        [AllowAnonymous]
        [Log(OperationType.QueryEntity)]
        public async Task<IActionResult> GetAssetsJZFL()
        {
            var actionResult = await this.Assets_infoService.GetAssetsJZFL();
            return Ok(actionResult);
        }


        /// <summary>
        /// 查询资产信息用途分类
        /// </summary>
        /// <returns>ActionResult</returns>
        [HttpPost]
        [AllowAnonymous]
        [Log(OperationType.QueryEntity)]
        public async Task<IActionResult> GetAssetsYT()
        {
            var actionResult = await this.Assets_infoService.GetAssetsYT();
            return Ok(actionResult);
        }


        /// <summary>
        /// 查询合同缴费信息
        /// </summary>
        /// <returns>ActionResult</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeeinfoData([FromBody] int count)
        {
            var actionResult = await this.Assets_infoService.GetFeeinfoData(count);
            return Ok(actionResult);
        }
    }
}
