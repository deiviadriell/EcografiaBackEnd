using System;
using System.Drawing;
using System.Net.Http.Headers;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPIEcoImagen;
using WebAPIEcoImagen.Models;
using LinqKit;


namespace WebAPIEcoImagen.Controllers
{   
    public class SeguimientoesController : ApiController
    {
        private BDEcoimagenEntities db = new BDEcoimagenEntities();

        // GET: api/Seguimientoes
        public IQueryable<Seguimiento> GetSeguimientoes()
        {
            return db.Seguimientoes.Where(x=>x.borrado==null);
        }

        // GET: api/Seguimientoes/5
        [ResponseType(typeof(Seguimiento))]
        public IHttpActionResult GetSeguimiento(int id)
        {
            Seguimiento seguimiento = db.Seguimientoes.Find(id);
            if (seguimiento == null)
            {
                return NotFound();
            }

            return Ok(seguimiento);
        }

        // PUT: api/Seguimientoes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutSeguimiento(int id, Seguimiento seguimiento)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != seguimiento.idSeguimiento)
            {
                return BadRequest();
            }

            db.Entry(seguimiento).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SeguimientoExists(id))
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

        // POST: api/Seguimientoes
        [ResponseType(typeof(Seguimiento))]
        public IHttpActionResult PostSeguimiento(Seguimiento seguimiento)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Seguimientoes.Add(seguimiento);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = seguimiento.idSeguimiento }, seguimiento);
        }
        [HttpPost]
        [Route("api/Seguimientoes/ReportePorFechas")]
        public IHttpActionResult GetSeguimientosPorFecha(JObject objeto)
        {
            var predicate = PredicateBuilder.New<Seguimiento>();
            var busqueda = objeto.ToObject<BusquedaSeguimientosFecha>();
            predicate = predicate.And(x => x.borrado == null);
            predicate = predicate.And(x => x.Paciente.borrado == null);
            if (busqueda.fechaInicio != null && busqueda.fechaFin != null)
            {
                var finMasUno = busqueda.fechaFin.Value.AddDays(1);
                predicate = predicate.And(t => busqueda.fechaInicio <= t.fecha && finMasUno >= t.fecha);
            }
            var seguimientos = db.Seguimientoes.Where(predicate).Select(x => new
            {
                idSeguimiento = x.idSeguimiento,
                fecha = x.fecha,
                paciente = x.Paciente.nombres + " " + x.Paciente.apellidos,
                tipo = x.tipoSeguimiento,
                sintomas = x.sintomas,
                medicoReferente=x.medicoReferente,

            }).Take(2000).ToList();

            return Ok(seguimientos);
        }
        [HttpPost]
        [Route("api/Seguimientoes/ReportePorFechas/Count")]
        public IHttpActionResult GetSeguimientosPorFechaCount(JObject objeto)
        {
            var predicate = PredicateBuilder.New<Seguimiento>();
            var busqueda = objeto.ToObject<BusquedaSeguimientosFecha>();
            predicate = predicate.And(x => x.borrado == null);
            predicate = predicate.And(x => x.Paciente.borrado == null);
            if (busqueda.fechaInicio != null && busqueda.fechaFin != null)
            {
                var finMasUno = busqueda.fechaFin.Value.AddDays(1);
                predicate = predicate.And(t => busqueda.fechaInicio <= t.fecha && finMasUno >= t.fecha);
            }
            var seguimientos = db.Seguimientoes.Where(predicate).Count();

            return Ok(seguimientos);
        }
        [HttpPost]
        [Route("api/Seguimientoes/Seguimientos/")]
        public IHttpActionResult GetSeguimientosSeguimientos(JObject objeto)
        {
            var predicate = PredicateBuilder.New<Seguimiento>();
            var busqueda = objeto.ToObject<BusquedaSeguimientos>();
            predicate = predicate.And(x => x.borrado == null);
            predicate = predicate.And(x => x.Paciente.borrado == null);
            switch (busqueda.tipo)
            {
                //paciente
                case 1:
                    predicate = predicate.And(x => x.Paciente.nombres.Contains(busqueda.texto) || x.Paciente.apellidos.Contains(busqueda.texto));
                    break;

                //cédula 
                case 2:
                    predicate = predicate.And(x => x.Paciente.cedula.Contains(busqueda.texto));
                    break;

                //correo
                case 3:
                    predicate = predicate.And(x => x.Paciente.correo.Contains(busqueda.texto));
                    break;

                //género
                case 4:
                    predicate = predicate.And(x => x.Paciente.genero == busqueda.texto);
                    break;               
                //teléfono
                case 5:
                    predicate = predicate.And(x => x.Paciente.telefono.Contains(busqueda.texto));
                    break;
                //fecha
                case 6:
                    DateTime fecha = Convert.ToDateTime(busqueda.texto);
                    predicate = predicate.And(x => x.fecha.Value.CompareTo(fecha)==1);

                    break;
                //sintomas
                case 7:
                    predicate = predicate.And(x => x.sintomas.Contains(busqueda.texto));
                    break;
                //Médico referente
                case 8:
                    predicate = predicate.And(x => x.medicoReferente.Contains(busqueda.texto));
                    break;
              

            }


            var seguimientos = db.Seguimientoes.Where(predicate).Select(x => new
            {
                idSeguimiento = x.idSeguimiento,
                fecha = x.fecha,
                paciente = x.Paciente.nombres + " " + x.Paciente.apellidos,
                cedula=x.Paciente.cedula,
                tipo = x.tipoSeguimiento,
                sintomas = x.sintomas,
                medicoReferente = x.medicoReferente,
            }).ToList();

            return Ok(seguimientos);
        }
        [HttpGet]
        [Route("api/Seguimientoes/Total")]
        public IHttpActionResult GetTotalSeguimientos()
        {
            var seguimientos= db.Seguimientoes.Where(x=>x.borrado==null && x.Paciente.borrado==null).Count();
            return Ok(seguimientos);
        }
        [HttpPost]
        [Route("api/Seguimientoes/Exportar/Excel/Avanzada/Todos/")]
        public HttpResponseMessage ExportExcelSeguimientosAvanzada(JObject objeto)
        {
            var lista = db.Seguimientoes.AsExpandable().Where(BusquedaAvanzadaSeguimiento(objeto))
                .Select(x => new
                {
                    fecha=x.fecha,
                    paciente = x.Paciente.nombres + " " + x.Paciente.apellidos,
                    direccion=x.Paciente.direccion,
                    telefono=x.Paciente.telefono,
                    medicoReferente=x.medicoReferente,
                    sintomas=x.sintomas,
                    tipo=x.tipoSeguimiento,
                }).ToList()
           .Select(x => new XlsxSeguimientosReporte
           {
               fecha= String.Format("{0:dd/MM/yyyy}", x.fecha),
               paciente=x.paciente,
               direccion=x.direccion,
               telefono=x.telefono,
               medicoReferente=x.medicoReferente,
               sintomas=x.sintomas,
               tipo=x.tipo,
           });

            
            var fileName = "ReporteSeguimientos" + DateTime.Now.ToString("MM-dd");
            ExcelPackage pck = new ExcelPackage();

            //Create the worksheet
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add(fileName);

            //get our column headings
            var t = typeof(XlsxSeguimientosReporte);
            var Headings = t.GetProperties();
            for (int i = 0; i < Headings.Count(); i++)
            {

                ws.Cells[1, i + 1].Value = Headings[i].Name;
            }

            //populate our Data
            if (lista.Count() > 0)
            {
                ws.Cells["A2"].LoadFromCollection(lista);
            }

            //Format the header
            using (ExcelRange rng = ws.Cells["A1:BI1"])
            {
                rng.Style.Font.Bold = true;
                rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                rng.Style.Font.Color.SetColor(Color.White);
                int columnIndex = 1;

                foreach (var cel in rng)
                {
                    ws.Column(columnIndex).AutoFit();
                    columnIndex++;
                }
            }


            // processing the stream.
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(pck.GetAsByteArray())
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.Add("x-filename", fileName + ".xlsx");
            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = fileName + ".xlsx";
            return result;

        }

        //DELETE: api/Seguimientoes/5
        [ResponseType(typeof(Seguimiento))]
        public IHttpActionResult DeleteSeguimiento(int id)
        {
            Seguimiento seguimiento = db.Seguimientoes.Find(id);
            if (seguimiento == null)
            {
                return NotFound();
            }

            db.Seguimientoes.Remove(seguimiento);
            db.SaveChanges();

            return Ok(seguimiento);
        }
        private ExpressionStarter<Seguimiento> BusquedaAvanzadaSeguimiento(JObject objeto)
        {
            var busqueda = objeto.ToObject<BusquedaSeguimientosFecha>();
            var predicate = PredicateBuilder.New<Seguimiento>();
            predicate = predicate.And(x => x.borrado == null);
            predicate = predicate.And(x => x.Paciente.borrado == null);
            if (busqueda.fechaInicio != null && busqueda.fechaFin != null)
            {
                var finMasUno = busqueda.fechaFin.Value.AddDays(1);
                predicate = predicate.And(t => busqueda.fechaInicio <= t.fecha && finMasUno >= t.fecha);
            }

            return (predicate);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SeguimientoExists(int id)
        {
            return db.Seguimientoes.Count(e => e.idSeguimiento == id) > 0;
        }
    }
}