﻿using EventShare.Data.Enums;
using EventShare.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventShare.Web.ViewModels
{
    public class Event
    {
        [Key]
        public string Id { get; set; }

        [Display(Name = "Title")]
        [Required]
        [StringLength(100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string Title { get; set; }

        [Display(Name = "Details")]
        [Required]
        [DataType(DataType.MultilineText)]
        [StringLength(100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string Details { get; set; }

        [Display(Name = "DateAndTime")]
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DateAndTime { get; set; }

        [Display(Name = "Status")]
        [Required]
        [EnumDataType(typeof(EventStatus))]
        public EventStatus Status { get; set; }

        public int LikersCount { get; set; }

        public bool Liked { get; set; }

        public ApplicationUser Publisher { get; set; }
    }
}
