﻿using System.Data.Entity.ModelConfiguration;

namespace ZHXY.Domain
{
    public class TeacherMap : EntityTypeConfiguration<Teacher>
    {
        public TeacherMap()
        {
            ToTable("School_Teachers");
            HasKey(t => t.F_Id);
            HasRequired(t => t.teacherSysUser)
                .WithMany()
                .HasForeignKey(t => t.F_User_ID)
                .WillCascadeOnDelete(false);
        }
    }
}