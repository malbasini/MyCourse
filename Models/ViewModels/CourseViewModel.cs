using System;
using System.Collections.Generic;
using MyCourse.Models.ValueObjects;
using System.Data;
using MyCourse.Models.Enums;



namespace MyCourse.Models.ViewModels;

public class CourseViewModel
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ImagePath { get; set; }
    public string? Author { get; set; }
    public double Rating { get; set; }
    public Money? FullPrice { get; set; }
    public Money? CurrentPrice { get; set; }

    public static CourseViewModel FromDataRow(DataRow row)
    {
        return new CourseViewModel
        {
            Id = Convert.ToInt32(row["Id"]),
            Title = row["Title"].ToString(),
            ImagePath = row["ImagePath"].ToString(),
            Author = row["Author"].ToString(),
            Rating = Convert.ToDouble(row["Rating"]),
            FullPrice =
                new Money((Currency)Enum.Parse(typeof(Currency), row["FullPrice_Currency"].ToString()),
                    Convert.ToDecimal(row["FullPrice_Amount"])),
            CurrentPrice =
                new Money((Currency)Enum.Parse(typeof(Currency), row["CurrentPrice_Currency"].ToString()),
                    Convert.ToDecimal(row["CurrentPrice_Amount"]))
        };
    }
}