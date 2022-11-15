﻿using NGProjectAdmin.Entity.BusinessEntity.NGBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGProjectAdmin.Entity.BusinessDTO.NGBusiness
{
    /// <summary>
    /// 资产档案列表
    /// </summary>
    public class Assets_infoDTO: Assets_info
    {
        public Assets_info_ContractDTO? contractinfo { get; set; }
        public Assets_info_AssetMentDTO? assetsMent { get; set; }
    }
}
