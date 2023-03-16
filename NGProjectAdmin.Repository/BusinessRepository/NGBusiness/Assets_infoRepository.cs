using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Nest;
using NGProjectAdmin.Common.Class.Exceptions;
using NGProjectAdmin.Entity.BusinessDTO.NGBusiness;
using NGProjectAdmin.Entity.BusinessEntity.BusinessModule;
using NGProjectAdmin.Entity.BusinessEntity.NGBusiness;
using NGProjectAdmin.Entity.CoreEntity;
using NGProjectAdmin.Repository.Base;
using NPOI.SS.Formula.Functions;
using SqlSugar;

namespace NGProjectAdmin.Repository.BusinessRepository.NGBusiness
{
    /// <summary>
    /// File_group数据访问层实现
    /// </summary>   
    public class Assets_infoRepository : BaseRepository<Assets_info>, IAssets_infoRepository
    {
        /// <summary>
        /// HttpContext
        /// </summary>
        private readonly IHttpContextAccessor context;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context"></param>
        public Assets_infoRepository(IHttpContextAccessor context) : base(context)
        {
            this.context = context;
        }

        public async Task<int> DeleteAssetAndContract(Assets_infoDTO assetId)
        {
            try
            {
                NGDbContext.BeginTran();
                Assets_info obj = await NGDbContext.Queryable<Assets_info>().Where(x => x.Id == assetId.Id).FirstAsync();

                // 删除对应的合同
                var contracts = await NGDbContext.Queryable<Contract_baseinfo>().Where(x => x.AssetsId == assetId.Id).ToListAsync();
                if (contracts != null)
                {
                    foreach (var item in contracts)
                    {
                        item.IsDel = 1;
                        await NGDbContext.Updateable(item).ExecuteCommandAsync();

                        // 删除对应的收费信息
                    }


                    foreach (var item in contracts)
                    {
                        var list = NGDbContext.Queryable<Contract_feeinfo>().Where(x => x.contractId == item.Id).ToListAsync();
                        if (list != null)
                        {

                            foreach (var key in list.Result)
                            {
                                key.IsDel = 1;
                                await NGDbContext.Updateable(key).ExecuteCommandAsync();
                            }
                        }
                    }
                }

                // 把资产档案删除

                obj.IsDel = 1;
                int res = await NGDbContext.Updateable(obj).ExecuteCommandAsync();
                NGDbContext.CommitTran();
                return res;
            }
            catch (Exception ex)
            {
                NGDbContext.RollbackTran();
                throw new NGAdminCustomException(ex.Message);
            }



        }

        public async Task<Assets_infoDTO> GetAssetByIdAsync(Assets_infoDTO assetId)
        {
            var asset = await NGDbContext.Queryable<Assets_info>()
           .LeftJoin<Contract_baseinfo>((a, c) => c.AssetsId == a.Id && a.IsDel == 0)
           .LeftJoin<Assetment_detail>((a, c, d) => a.AssetsMentGroupId == d.AssetMentId)
           .LeftJoin<Assetment_group>((a, c, d, e) => a.AssetsMentGroupId == e.Id)
            .Select((a, c, d, e) => new Assets_infoDTO()
            {
                propertyFileGroupId = a.propertyFileGroupId,
                LandPropertyInfo = a.LandPropertyInfo,
                landCode = a.landCode,
                PropertyCode = a.PropertyCode,
                AssetsFileGroupId = a.AssetsFileGroupId,
                contract_groupId = a.contract_groupId,
                AssetsFor = a.AssetsFor,
                PropertyOwner = a.PropertyOwner,
                Id = a.Id,
                AssetsCode = a.AssetsCode,
                AssetsTypeId = a.AssetsTypeId,
                AssetsSourceId = a.AssetsSourceId,
                AssetsState = a.AssetsState,
                AssetsArea = a.AssetsArea,
                AssetsAdress = a.AssetsAdress,
                AssetUseType = a.AssetUseType,
                MapInfo = a.MapInfo,
                gyqk = a.gyqk,
                bdcdyh = a.bdcdyh,
                qlxz = a.qlxz,
                yt = a.yt,
                tdsymj = a.tdsymj,
                fwjzmj = a.fwjzmj,
                qx = a.qx,
                bgtime = a.bgtime,
                endtime = a.endtime,
                cs = a.cs,
                fwjg = a.fwjg,
                tdmj = a.tdmj,
                tdpgjz = a.tdpgjz,
                fwpgjz = a.fwpgjz,
                fwmj = a.fwmj,
                dyqr = a.dyqr,
                dyje = a.dyje,
                dyqx = a.dyqx,
                xcsj1 = a.xcsj1,
                xcqk1 = a.xcqk1,
                xcsj2 = a.xcsj2,
                xcqk2 = a.xcqk2,
                xcsj3 = a.xcsj3,
                xcqk3 = a.xcqk3,
                remarks = a.remarks,
                assetsName = a.assetsName,
                jsnd = a.jsnd,
                assetsMent = new Assets_info_AssetMentDTO()
                {
                    buildDate = e.BuildDate,
                    assessArea = d.AssessArea,
                    assetPriceOneYear = d.AssetPriceOneYear,
                    id = d.Id,
                    AssetMentId = d.AssetMentId,
                    assetCode = e.AssetCode
                }
            })
            .FirstAsync(a => a.Id.Equals(assetId.Id));
            asset.assetDate = new List<DateTime>() { Convert.ToDateTime(asset.bgtime), Convert.ToDateTime(asset.endtime) };

            List<Assets_info_ContractDTO> ls = NGDbContext.Queryable<Contract_baseinfo>().Where(x => x.AssetsId == asset.Id && x.contract_groupId == asset.contract_groupId)
              .Select(c => new Assets_info_ContractDTO()
              {
                  remark = c.Remark,
                  contractState = c.ContractState,
                  contractPromiseMoney = c.ContractPromiseMoney,
                  contractLife = c.ContractLife,
                  contractPayment = c.ContractPayment,
                  ContractPdfGroupId = c.ContractPdfGroupId,
                  lesseeId = c.lesseeId,
                  lesseeAdress = c.lesseeAdress,
                  lessorId = c.lessorId,
                  lessorAdress = c.LessorAdress,
                  lessor = c.Lessor,
                  lessorPhone = c.lessorPhone,
                  contractDate = c.ContractDate,
                  contractType = c.ContractType,
                  id = c.Id,
                  lessee = c.lessee,
                  lesseePhone = c.lesseePhone,
                  ContracStartDate = c.ContracStartDate,
                  ContractEndDate = c.ContractEndDate,
                  ContractPrice = c.ContractPrice,
                  ContractMoney = c.ContractMoney
              }).ToList();

            List<Assets_info_ContractDTO?> ls1 = new List<Assets_info_ContractDTO?>();
            if (ls != null && ls.Count > 0)
            {
                foreach (var item in ls)
                {
                    ls1.Add(item);
                }
            }
            asset.contractinfo = ls1;
            return asset;
        }

        public async Task<List<Assets_infoDTO>> GetAssetInfoListAsync(QueryCondition queryCondition, RefAsync<int> totalCount)
        {
            var where = QueryCondition.BuildExpression<Assets_info>(queryCondition.QueryItems);

            var list = await NGDbContext.
               Queryable<Assets_info>().
               WhereIF(true, where).
               Where(a => a.IsDel == 0).
               OrderByIF(!String.IsNullOrEmpty(queryCondition.Sort), queryCondition.Sort).
               Select((a) => new Assets_infoDTO() { Id = a.Id, assetsName = a.assetsName, AssetsCode = a.AssetsCode, AssetsTypeId = a.AssetsTypeId, AssetsState = a.AssetsState, AssetsArea = a.tdsymj, AssetsAdress = a.AssetsAdress, AssetUseType = a.AssetUseType }).
               ToPageListAsync(queryCondition.PageIndex, queryCondition.PageSize, totalCount);

            foreach (Assets_infoDTO item in list)
            {
                if (item.contractinfoMain != null)
                {
                    item.contractinfo = new List<Assets_info_ContractDTO?>() { item.contractinfoMain };
                }

            }
            return list;
        }

        public async Task<AssetsDataDTO> GetAssetsData()
        {
            AssetsDataDTO assetsDataDTO = new AssetsDataDTO();
            try
            {
                assetsDataDTO.AssetsCount = await NGDbContext.Queryable<Assets_info>().Where(x => x.IsDel == 0).CountAsync();
                assetsDataDTO.validContract = await NGDbContext.Queryable<Contract_baseinfo>().Where(x => x.IsDel == 0 && x.ContractEndDate > DateTime.Now).CountAsync();
                assetsDataDTO.invalidContract= await NGDbContext.Queryable<Contract_baseinfo>().Where(x => x.IsDel == 0 && x.ContractEndDate < DateTime.Now).CountAsync();
                assetsDataDTO.mouthCloseContract= await NGDbContext.Queryable<Contract_baseinfo>().Where(x => x.IsDel == 0 && x.ContractEndDate.Month == DateTime.Now.Month).CountAsync();

            }
            catch (Exception ex)
            {

                throw new NGAdminCustomException(ex.Message);
            }

            return assetsDataDTO;
        }
    }
}
