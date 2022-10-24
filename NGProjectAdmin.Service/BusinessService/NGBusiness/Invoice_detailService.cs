﻿using NGProjectAdmin.Entity.BusinessEntity.BusinessModule;
using NGProjectAdmin.Entity.BusinessEntity.NGBusiness;
using NGProjectAdmin.Entity.CoreEntity;
using NGProjectAdmin.Repository.Base;
using NGProjectAdmin.Repository.BusinessRepository.NGBusiness;
using NGProjectAdmin.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGProjectAdmin.Service.BusinessService.NGBusiness
{
    public class Invoice_detailService : BaseService<Invoice_detail>, IInvoice_detailService
    {
        #region 属性及其构造函数   
        /// <summary>
        /// 仓储实例
        /// </summary>
        private readonly IInvoice_detailRepository Invoice_detailRepository;

        public Invoice_detailService(IInvoice_detailRepository Invoice_detailRepository) : base(Invoice_detailRepository)
        {
            this.Invoice_detailRepository = Invoice_detailRepository;
        }
        #endregion

    }
}
