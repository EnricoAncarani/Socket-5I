using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
namespace Socket_4I
{
    public class Contatto
    {
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public IPEndPoint Indirizzo { get; set; }

        public Contatto(string nome, string cognome, IPEndPoint indirizzo)
        {
            Nome = nome;
            Cognome = cognome;
            Indirizzo = indirizzo;
        }
        // Metodo che restituisce una stringa rappresentante il contatto
        public override string ToString()
        {
            return Nome + " " + Cognome + " - indirizzo: " + Indirizzo;
        }
    }
}
