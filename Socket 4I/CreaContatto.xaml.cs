using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
namespace Socket_4I
{
    /// <summary>
    /// Logica di interazione per CreaContatto.xaml
    /// </summary>
    public partial class CreaContatto : Window
    {
        List<Contatto> listaContatti;
        public CreaContatto(List<Contatto> ListaContatti)
        {
            listaContatti = ListaContatti;
            InitializeComponent();
        }

        private void AggiungiContatto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(Txtnome.Text== "Broadcast")
                {
                    MessageBox.Show("Non puoi usare questo nome per un contatto");
                }
                if(IndirizzoLocale.IsChecked==true)
                {
                    
                    Contatto contatto = new Contatto(Txtnome.Text, txtCognome.Text, new IPEndPoint(IPAddress.Parse("127.0.0.1"), int.Parse(TxtPorta.Text)));
                    listaContatti.Add(contatto);
                }
                else
                {
                    Contatto contatto = new Contatto(Txtnome.Text, txtCognome.Text, new IPEndPoint(IPAddress.Parse(TxtIndirizzo.Text), int.Parse(TxtPorta.Text)));
                    listaContatti.Add(contatto);
                }
                
                Close();
            }catch
            {
                MessageBox.Show("Valori per il contatto non validi");
            }
            
        }

        private void IndirizzoLocale_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
