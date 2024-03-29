﻿using NGProjectAdmin.Entity.Base;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGProjectAdmin.Entity.BusinessEntity.NGBusiness
{
    /// <summary>
    /// 合同基础信息
    /// </summary>
    [SugarTable("contract_baseinfo")]
    public class Contract_baseinfo: BaseEntity
    {
        /// <summary>
        /// 合同编号
        /// </summary>
        public string? ContractCode { get; set; }

        /// <summary>
        /// 合同签订日期
        /// </summary>
        public DateTime ContractDate { get; set; }=new DateTime(DateTime.Now.Ticks);

        /// <summary>
        /// 合同类型 0公开租牌合同1协议租赁合同2出借协议
        /// </summary>
        public int ContractType { get; set; }=0;

        /// <summary>
        /// 出租方（甲方）
        /// </summary>
        public string? Lessor { get; set; }

        /// <summary>
        /// 甲方地址
        /// </summary>
        public string? LessorAdress { get; set; }

        /// <summary>
        /// 承租方（乙方）
        /// </summary>
        public string? lessee { get; set; }

        public string? lessorPhone { get; set; }

        /// <summary>
        /// 乙方身份证号码（统一社会信用代码）
        /// </summary>
        public string? lesseeId { get; set; }

        /// <summary>
        /// 甲方身份证号码（统一社会信用代码）
        /// </summary>
        public string? lessorId { get; set; }

        /// <summary>
        /// 乙方联系方式
        /// </summary>
        public string? lesseePhone { get; set; }

        /// <summary>
        /// 乙方地址
        /// </summary>
        public string? lesseeAdress { get; set; }

        /// <summary>
        /// 担保方
        /// </summary>
        public string? Warrant { get; set; }

        /// <summary>
        /// 担保方身份证号码（统一社会信用代码）
        /// </summary>
        public string? warrantId { get; set; }

        /// <summary>
        /// 担保方联系方式
        /// </summary>
        public string? warrantPhone { get; set; }

        /// <summary>
        /// 担保方地址
        /// </summary>
        public string? warrantAdress { get; set; }

        /// <summary>
        /// 合同总租期（年）
        /// </summary>
        public int ContractLife { get; set; } = 0;

        /// <summary>
        /// 租期起始日期
        /// </summary>
        public DateTime ContracStartDate { get; set; } = new DateTime();

        /// <summary>
        /// 租期终止日期
        /// </summary>
        public DateTime ContractEndDate { get; set; } = new DateTime();

        /// <summary>
        /// 合同年租金
        /// </summary>
        public double ContractPrice { get; set; } = 0;

        /// <summary>
        /// 合同总金额
        /// </summary>
        public double ContractMoney { get; set; } = 0;  

        /// <summary>
        /// 合同保证金
        /// </summary>
        public double ContractPromiseMoney { get; set; }=0;

        /// <summary>
        /// 0半年一交1一年一交
        /// </summary>
        public int ContractPayment { get; set; } = 0;

        /// <summary>
        /// 合同附件表Id
        /// </summary>

        public string ContractPdfGroupId { get; set; } = "";

        /// <summary>
        /// 合同附件信息
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(ContractPdfGroupId), nameof(File_group.Id))]
        public File_group? file_group { get; set; }

        /// <summary>
        /// 合同信息备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 资产信息ID
        /// </summary>
        public string? AssetsId { get; set; }

        /// <summary>
        /// 资产信息
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(AssetsId), nameof(Assets_group.Id))]
        public Assets_group? assets_groupinfo { get; set; }

        /// <summary>
        /// 租金收款组Id
        /// </summary>
        public string? RentGroupId { get; set; }

        /// <summary>
        /// 资产信息
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(RentGroupId), nameof(Assets_group.Id))]
        public Assets_group? RentGroupfiles{ get; set; }

        /// <summary>
        /// 开票登记组Id
        /// </summary>
        public string? InvoiceGroupId { get; set; }

        /// <summary>
        /// 开票登记信息
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(InvoiceGroupId), nameof(Invoice_group.Id))]
        public Invoice_group? invoice_group { get; set; }

        /// <summary>
        /// 保证金违约金信息表Id
        /// </summary>
        public string? ContractDefaultId { get; set; }


        /// <summary>
        /// 保证金违约金信息
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(ContractDefaultId), nameof(Contract_default.Id))]
        public Contract_default? contract_default { get; set; }

        /// <summary>
        /// 财务信息Id
        /// </summary>
        public string? FinanceId { get; set; }

        /// <summary>
        /// 保证金违约金信息
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(FinanceId), nameof(Finance_info.Id))]
        public Finance_info? finance_info { get; set; }

        /// <summary>
        /// 0合同完结1合同逾期2不存在逾期
        /// </summary>
        public int ContractState { get; set; }=0;

        /// <summary>
        /// 对应合同组
        /// </summary>
        public Guid? contract_groupId { get; set; }
       
    }
}
