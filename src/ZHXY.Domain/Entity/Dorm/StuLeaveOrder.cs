﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZHXY.Domain
{
    /// <summary>
    /// 学生请假
    /// </summary>
    public class StuLeaveOrder : CompleteEntity
    {
        /// <summary>
        /// 申请人id
        /// </summary>
        public string ApplicantId { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndOfTime { get; set; }

        /// <summary>
        /// 请假学生的id
        /// </summary>
        public string LeaveerId { get; set; }

        /// <summary>
        /// 班主任id
        /// </summary>
        public string HeadTeacherId { get; set; }

        /// <summary>
        /// 请假天数
        /// </summary>
        public string LeaveDays { get; set; }
      
        [NotMapped]
        public decimal Days { get { return Convert.ToDecimal(LeaveDays); } }

        /// <summary>
        /// 请假类型
        /// </summary>
        public string LeaveType { get; set; }

        /// <summary>
        /// 请假理由
        /// </summary>
        public string ReasonForLeave { get; set; }

        /// <summary>
        /// 状态(0:未审批 1:已审批)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 审批意见
        /// </summary>

        public string ApprovalOpinion { get; set; }

        /// <summary>
        /// 班主任
        /// </summary>
        public virtual User HeadTeacher { get; set; }

        /// <summary>
        /// 请假人
        /// </summary>
        public virtual User Leaveer { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public virtual User Applicant { get; set; }
    }
}