﻿using ZHXY.Domain.Entity;
using System.Data.Entity.ModelConfiguration;

namespace ZHXY.Mapping
{
    public class ItemsDetailMap : EntityTypeConfiguration<SysDicItem>
    {
        public ItemsDetailMap()
        {
            ToTable("zhxy_item_detail");
            HasKey(t => t.F_Id);
        }
    }
}