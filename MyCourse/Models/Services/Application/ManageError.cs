using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Exceptions;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
    public class ManageError
    {
        public string ViewName { get; internal set; }
        public int StatusCode { get; internal set; }

        public string Title { get; internal set; }
        private readonly IExceptionHandlerPathFeature feature;

        public ManageError(IExceptionHandlerPathFeature feature)
        {
            this.feature = feature;
        }
        public void Manage()
        {
            switch (feature.Error)
            {
                case CourseNotFoundException ex:
                    {
                        Title = "Corso non trovato.";
                        StatusCode = 404;
                        ViewName = "CourseNotFound";
                        break;
                    }
                default:
                    {
                        Title = "Errore.";
                        StatusCode = 500;
                        ViewName = "Index";
                        break;
                    }
            }
        }
    }
}