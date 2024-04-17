using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MyCourse.Models.Services.Infrastructure
{
    public class InsecureImagePersister : IImagePersister
    {
        private readonly IWebHostEnvironment env;

        public InsecureImagePersister(IWebHostEnvironment env)
        {
            this.env = env;
        }
        /*
         * SaveCourseImageAsync` ha due parametri: `int courseId` e `IFormFile formFile`. "courseId" è un numero intero
         * che rappresenta l'ID univoco di un corso. `formFile` è un oggetto che rappresenta un file
         * caricato HTTP (elencato nei dati del modulo).
           - Il metodo innanzitutto costruisce sia un percorso logico (`path`, che deve essere restituito dal metodo) 
           sia un percorso fisico (`physicalPath`, che rappresenta dove il file verrà salvato sul disco).
           - Quindi recupera un `FileStream` che consente di scrivere sul percorso fisico utilizzando `File.OpenWrite`.
           - Viene quindi utilizzato `formFile.CopyToAsync(fileStream)` che copia il contenuto del file 
           caricato in `FileStream`. Questa è un'operazione asincrona, quindi `await` viene utilizzato
            per consentire al metodo di restituire il controllo al chiamante e successivamente riprendere 
            una volta eseguita questa operazione.
           - Infine, "SaveCourseImageAsync" restituisce il percorso logico al nuovo file.
           
         */
        public async Task<string> SaveCourseImageAsync(int courseId, IFormFile formFile)
        {
            //Salvare il file
            string path = $"/Courses/{courseId}.jpg";
            string physicalPath = Path.Combine(env.WebRootPath, "Courses", $"{courseId}.jpg");
            using FileStream fileStream = File.OpenWrite(physicalPath);
            await formFile.CopyToAsync(fileStream);

            //Restituire il percorso al file
            return path;
        }
    }
}