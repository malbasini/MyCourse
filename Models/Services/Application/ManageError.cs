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
        private readonly HttpContext context;

        public string ViewName { get; internal set; }
        public int StatusCode { get; internal set; }

        public string Title { get; internal set; }
        public ManageError(HttpContext context)
        {
            this.context = context;
        }
        public void Manage()
        {
            var feature = context.Features.Get<IExceptionHandlerPathFeature>();
            switch (feature.Error)
            {
                case CourseNotFoundException ex:
                {
                    Title = "Corso non trovato";
                    StatusCode = 404;
                    ViewName = "CourseNotFound";
                    break;
                }
                case Microsoft.Data.Sqlite.SqliteException exc:
                {
                    Title = "Errore Database Sqlite";
                    StatusCode = 500;
                    ViewName = "DatabaseSqliteError";
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