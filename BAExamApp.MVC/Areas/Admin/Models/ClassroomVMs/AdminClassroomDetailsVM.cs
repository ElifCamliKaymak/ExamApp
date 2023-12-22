﻿using BAExamApp.Dtos.StudentClassrooms;
using BAExamApp.Dtos.TrainerClassrooms;
using System.ComponentModel.DataAnnotations;

namespace BAExamApp.MVC.Areas.Admin.Models.ClassroomVMs;

public class AdminClassroomDetailsVM
{
    public Guid Id { get; set; }

    [Display(Name = "Classroom")]
    public string Name { get; set; }

    [Display(Name = "Opening_Date")]
    public DateTime OpeningDate { get; set; }

    [Display(Name = "Closed_Date")]
    public DateTime ClosedDate { get; set; }

    [Display(Name = "Group_Type")]
    public string GroupTypeName { get; set; }

    [Display(Name = "Branch_Id")]
    public string BranchName { get; set; }

    [Display(Name = "Product_Name")]
    public List<string> ProductNames { get; set; }

    [Display(Name = "Class_Grade_Average")]
    public decimal? ClassGradeAverage { get; set; }

    [Display(Name = "Student_Count")]
    public int? StudentCount { get; set; }

    [Display(Name = "Student_Exam_Score")]
    public int? StudentExamScore { get; set; }

    [Display(Name = "Student_Exam_Score_Count")]
    public int? StudentExamScoreCount { get; set; }


    public List<StudentClassroomListForClassroomDetailsForAdminDto> StudentClassrooms { get; set; }
    public List<TrainerClassroomListForClassroomDetailsDto> TrainerClassrooms { get; set; }
}
