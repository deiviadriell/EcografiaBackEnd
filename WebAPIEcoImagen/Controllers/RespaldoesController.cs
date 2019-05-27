using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPIEcoImagen;
using WebAPIEcoImagen.Models;
using CG.Web.MegaApiClient;
namespace WebAPIEcoImagen.Controllers
{
    public class RespaldoesController : ApiController
    {
        //verifico si tiene conexxion a internet
        string estado = "";
        string archivoSubir = "";
        MegaApiClient cliente = new MegaApiClient();
        //generar archivo sql localmente
        private BDEcoimagenEntities db = new BDEcoimagenEntities();

        // GET: api/Respaldoes
        public IQueryable<Respaldo> GetRespaldoes()
        {
            return db.Respaldoes;
        }

        // GET: api/Respaldoes/5
        [ResponseType(typeof(Respaldo))]
        public IHttpActionResult GetRespaldo(int id)
        {
            Respaldo respaldo = db.Respaldoes.Find(id);
            if (respaldo == null)
            {
                return NotFound();
            }

            return Ok(respaldo);
        }
        [HttpGet]
        [Route("api/Respaldoes/VerificarDia/")]
        public IHttpActionResult verificarRespaldoDiario()
        {
            var respaldo = db.Respaldoes.Where(x => x.fechaCreacion.Value.Day==DateTime.Now.Day && x.fechaCreacion.Value.Month==DateTime.Now.Month && x.fechaCreacion.Value.Year==DateTime.Now.Year).Count();
            return Ok(respaldo);


        }
        [HttpPost]
        [Route("api/Respaldoes/ListaRespaldosMega/")]
        public IHttpActionResult getRespaldosFromMega()
        {
            int ano = DateTime.Now.Year;
            if(!cliente.IsLoggedIn)
                cliente.Login("xyz", "xyz");
            IEnumerable<INode> nodes = cliente.GetNodes();
            List<INode> folders = nodes.Where(n => n.Type == NodeType.Directory).ToList();
            INode myFolder = folders.FirstOrDefault(f => f.Name == "Respaldos"+ano);
            IEnumerable<INode> folder = cliente.GetNodes(myFolder);
            List<INode> allFiles = folder.Where(n => n.Type == NodeType.File).ToList();

            var respaldos = allFiles.Select(x => new
            {                
                enlaceDescarga =cliente.GetDownloadLink(x).AbsoluteUri,
                fechaCreacion=x.CreationDate,
                fechaSubida=x.ModificationDate,
                tamano=x.Size,
                nombreArchivo=x.Name,
            }).ToList();
            return Ok(respaldos);

         
        }
        // PUT: api/Respaldoes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutRespaldo(int id, Respaldo respaldo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != respaldo.idRespaldo)
            {
                return BadRequest();
            }

            db.Entry(respaldo).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RespaldoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Respaldoes
        [ResponseType(typeof(Respaldo))]
        public IHttpActionResult PostRespaldo(Respaldo respaldo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //subo el archivo a mega

            db.Respaldoes.Add(respaldo);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = respaldo.idRespaldo }, respaldo);
        }
        [HttpPost]
        [Route("api/Respaldoes/SubirRespaldoAMega/")]
        public IHttpActionResult PostSubirRespaldoMega()
        {
            string dia=DateTime.Now.ToString("dddd", CultureInfo.CreateSpecificCulture("es-ES"));
            //return Ok(dia);
            //respaldos se van a realizar los miercoles y sabados 
            if (dia != "miércoles" && dia!= "sábado")            
                return BadRequest("Hoy no se realiza respaldos");            
            
            //verifico si hay internet
            try
            {
                System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry("www.google.com");
                estado = "okInternet";

            }
            catch
            {
                return BadRequest("No hay conexion a internet el respaldo del día de hoy no se ha podido realizar. Contacte con el administrador del sistema");
            }

            try
            {
                
                //generar archivo sql server
                var generacionLocal = db.backupdb().Select(x => new backupdb_Result
                {
                    nombre = x.nombre,
                    ruta = x.ruta,
                }).FirstOrDefault();
                string rutaArchivo = generacionLocal.ruta;
                string nombreArchivo = generacionLocal.nombre;
                int ano = DateTime.Now.Year;
                Respaldo resultado = new Respaldo();
                //resultado.enlaceDescarga = downloadUrl.AbsolutePath;
                resultado.fechaCreacion = DateTime.Now;
                resultado.fechaSubida = DateTime.Now;
                resultado.nodoContenedor = "Respaldos" + ano;
                resultado.nombreArchivo = nombreArchivo;
                resultado.rutaFisica = rutaArchivo;
                db.Respaldoes.Add(resultado);
                db.SaveChanges();
                cliente.Login("xyz", "xyz");
                var nodos = cliente.GetNodes(); bool existe = cliente.GetNodes().Any(n => n.Name == "Respaldos"+ano);
               
                resultado.nodoContenedor = "Respaldos"+ano;
                // Crear dos nodos.
                INode root;
                INode carpeta;
                // Si el directorio facturas existe, se obtiene. Si no existe, se crea.
                if (existe == true)
                    carpeta = nodos.Single(n => n.Name == "Respaldos" + ano);
                else
                {
                    //Obtenemos el nodo raíz.
                    root = nodos.Single(n => n.Type == NodeType.Root);
                    // Creamos el directorio llamado "Facturas" en la raíz.
                    carpeta = cliente.CreateFolder("Respaldos" + ano, root);
                }

                
                INode archivo = cliente.UploadFile(rutaArchivo, carpeta);
                // Obtener el link de descarga del archivo subido por si se requiere para algo.
                //Uri downloadUrl = cliente.GetDownloadLink(archivo);
                //IEnumerable<INode> nodes = cliente.GetNodes();
                //List<INode> folders = nodes.Where(n => n.Type == NodeType.Directory).ToList();
                //INode myFolder = folders.FirstOrDefault(f => f.Name == "Respaldos" + ano);
                //IEnumerable<INode> folder = cliente.GetNodes(myFolder);
                //List<INode> allFiles = folder.Where(n => n.Type == NodeType.File).ToList();
                //INode archivoSubido = allFiles.Where(n => n.Name == "nombreArchivo").FirstOrDefault();
                estado = "ok";
                cliente.Logout();

                return Ok("ok");

            }
            catch (Exception error)
            {
                estado = "error";
                //t.Abort();
                return BadRequest("Error al intentar subir el respaldo contacte con el administrador del sistema");

            }

        }

        [HttpPost]
        [Route("api/Respaldoes/SubirRespaldoAMegaInstante/")]
        public IHttpActionResult PostSubirRespaldoMegaInstante()
        {

            //verifico si hay internet
            if (!verificarInternet())
                return BadRequest("No hay Conexión a internet");            

            try
            {
                //respaldo cada sabado y miercoles
                

                //generar archivo sql server
                var generacionLocal = db.backupdb().Select(x => new backupdb_Result
                {
                    nombre = x.nombre,
                    ruta = x.ruta,
                }).FirstOrDefault();
                string rutaArchivo = generacionLocal.ruta;
                string nombreArchivo = generacionLocal.nombre;

                
                Respaldo resultado = new Respaldo();
                //resultado.enlaceDescarga = downloadUrl.AbsolutePath;
                resultado.fechaCreacion = DateTime.Now; ;
                //resultado.fechaSubida = archivoSubido.ModificationDate;
                resultado.nodoContenedor = "Respaldos";
                //resultado.tamano = (int)archivoSubido.Size;
                resultado.rutaFisica = rutaArchivo;
                resultado.estado = "subiendo a la nube...";
                db.Respaldoes.Add(resultado);
                db.SaveChanges();
                cliente.Login("xyz", "xyz");
                var nodos = cliente.GetNodes(); bool existe = cliente.GetNodes().Any(n => n.Name == "Respaldos");
                resultado.nodoContenedor = "Respaldos";
                // Crear dos nodos.
                INode root;
                INode carpeta;
                // Si el directorio facturas existe, se obtiene. Si no existe, se crea.
                if (existe == true)
                    carpeta = nodos.Single(n => n.Name == "Respaldos");
                else
                {
                    //Obtenemos el nodo raíz.
                    root = nodos.Single(n => n.Type == NodeType.Root);
                    // Creamos el directorio llamado "Facturas" en la raíz.
                    carpeta = cliente.CreateFolder("Respaldos", root);
                }

                INode archivo = cliente.UploadFile(rutaArchivo, carpeta);
                // Obtener el link de descarga del archivo subido por si se requiere para algo.
                Uri downloadUrl = cliente.GetDownloadLink(archivo);
                IEnumerable<INode> nodes = cliente.GetNodes();
                List<INode> folders = nodes.Where(n => n.Type == NodeType.Directory).ToList();
                INode myFolder = folders.FirstOrDefault(f => f.Name == "Respaldos");
                IEnumerable<INode> folder = cliente.GetNodes(myFolder);
                List<INode> allFiles = folder.Where(n => n.Type == NodeType.File).ToList();
                INode archivoSubido = allFiles.Where(n => n.Name == "nombreArchivo").FirstOrDefault();
                
                    int idRespaldo = resultado.idRespaldo;
                    resultado.estado = "subido";
                    resultado.enlaceDescarga = downloadUrl.AbsolutePath;
                    resultado.fechaSubida = archivoSubido.ModificationDate;
                

                //resultado.tamano = (int)archivoSubido.Size;

               // PutRespaldo(idRespaldo, resultado);
                estado = "ok";
                cliente.Logout();

                return Ok(resultado);

            }
            catch (Exception error)
            {
                estado = "error";
                //t.Abort();
                return BadRequest(error.Message);

            }

        }


        // DELETE: api/Respaldoes/5
        [ResponseType(typeof(Respaldo))]
        public IHttpActionResult DeleteRespaldo(int id)
        {
            Respaldo respaldo = db.Respaldoes.Find(id);
            if (respaldo == null)
            {
                return NotFound();
            }

            db.Respaldoes.Remove(respaldo);
            db.SaveChanges();

            return Ok(respaldo);
        }
        private bool verificarInternet()
        {
            try
            {
                System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry("www.google.com");
                return true;

            }
            catch
            {
                return false;
            }
        }
        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool RespaldoExists(int id)
        {
            return db.Respaldoes.Count(e => e.idRespaldo == id) > 0;
        }
    }
}
