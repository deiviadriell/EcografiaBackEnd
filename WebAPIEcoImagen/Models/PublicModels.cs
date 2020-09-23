using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPIEcoImagen.Models
{
    class PublicModels
    {
    }
    public class BusquedaSeguimientosFecha {
       public DateTime? fechaInicio { get; set; }
        public DateTime? fechaFin { get; set; }

    }
    public class BusquedaRespaldo
    {
        public DateTime? fecha { get; set; }        

    }
    public class BusquedaSeguimientos
    {
        public int tipo { get; set; }
        public string texto { get; set; }        

    }
    public class ResultadoRespaldo
    {
        public DateTime? fechaCreacion { get; set; }
        public DateTime? fechaSubida { get; set; }
        public string nombreArchivo { get; set; }
        public string rutaFisica { get; set; }
        public int? tamano { get; set; }
        public string enlaceDescarga { get; set; }
        public string nodoContenedor { get; set; }
    }
    public class ResultadoProcedimiento
    {
        public string ruta { get; set; }
        public string nombre { get; set; }
    }
    public partial class XlsxSeguimientosReporte
    {
        public string fecha { get; set; }
        public string paciente { get; set; }
        public string direccion { get; set; }
        public string telefono { get; set; }
        public string medicoReferente { get; set; }        
        public string tipo { get; set; }
        public string sintomas { get; set; }

    }
    public class PacientesMesesNumero
    {
        public List<string> meses { get; set; }        
        public List<int> datos { get; set; }
    }
    public class PacientesReciente
    {
        public int idPaciente { get; set; }
        public string cedula { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string direccion { get; set; }
        public string telefono { get; set; }
        public string correo { get; set; }
        public DateTime? fecNacimiento { get; set; }
        public string genero { get; set; }
        public DateTime? borrado { get; set; }
        public DateTime? fechaUltimaConsulta { get; set; }
    }

}
