using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using System.IO;

namespace Socket_4I
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Creazione di un'istanza della classe Socket e un'istanza per tutti gli strumenti che andrò a utilizzare
        List<Contatto> ListaContatti;
        Socket socket = null;
        Thread ControllaMessaggi;
        List<string> ListaMessaggi;
        IPAddress local_address;
        IPEndPoint local_endpoint;
        public MainWindow()
        {
            //Inizializazione della finestra
            InitializeComponent();
            //Genero le liste e il thread
            ListaMessaggi = new List<string>();
            ControllaMessaggi = new Thread(ControllaSeArrivatoMessaggio);
            ListaContatti = new List<Contatto>();
            //Uso i metodi di base per iniziare a generare la rubrica e i messaggi
            CreaIndirizzoDiBroadcast();
            CaricaContatti();
            CaricaMessaggi();
            //Genero una nuova socket con una determinata porta
            string Porta = "11000";
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); // Inizializzazione del socket con dei valori di base e il protocollo di comunicazione udp

            // Indirizzo IP e porta su cui il socket esegue il binding
            local_address = IPAddress.Any;           
            local_endpoint = new IPEndPoint(local_address, int.Parse(Porta));
            socket.Bind(local_endpoint); //Connetto la socket all'indirizzo
            
            // Impostazione degli attributi del socket
            socket.Blocking = false;
            socket.EnableBroadcast = true;
            //Faccio partire il thread che continua a controllare se arrivano messaggi
            ControllaMessaggi.Start();
            //In caso di chiusura della finestra chiudo anche il programma catturando l'evento
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Prima di chiudersi il programma salva i contatti e i messaggi poi chiude il thread e infine chiude tutto il progetto
            SalvaContatti(ListaContatti);
            SalvaMessaggi();
            ControllaMessaggi.Abort();
            Application.Current.Shutdown();
        }

        private void ControllaSeArrivatoMessaggio()
        {
            //Creo un contatto di base sconosciuto se l'indirizzo da cui mi arriva il messaggio non è salvato nella rubrica
            Contatto ContattoInteressato=new Contatto("Sconosciuto","",new IPEndPoint(IPAddress.Any,int.Parse("11000")));
            int nBytes = 0;
            //ripeto all'infinito
            while(true)
            {
                
                // Controllo se ci sono dati in arrivo sul socket
                bool riuscito=false;
                if ((nBytes = socket.Available) > 0) //se ci sono dati che stanno entrando dal socket entro e li controllo
                {
                    try
                    {
                        //ricezione dei caratteri in attesa
                        byte[] buffer = new byte[nBytes];

                        EndPoint remoreEndPoint = new IPEndPoint(IPAddress.Any, 0);

                        nBytes = socket.ReceiveFrom(buffer, ref remoreEndPoint);

                        // Estrazione dell'indirizzo IP del mittente e del messaggio ricevuto
                        string from = ((IPEndPoint)remoreEndPoint).ToString();
                        string messaggio = Encoding.UTF8.GetString(buffer, 0, nBytes); 
                        //Controllo quale contatto l'ha mandato
                        foreach(Contatto c in ListaContatti)
                        {
                            if(c.Indirizzo.ToString()==from)
                            {
                                ListaMessaggi.Add(""+c.Nome+" "+c.Cognome+": "+messaggio);
                                ContattoInteressato=c;
                                AggiornoLista();
                                riuscito=true;
                            }
                        }
                        if(riuscito==false) //se non è nessuno nella rubrica ci metto lo sconosciuto
                        {
                            ListaMessaggi.Add("" + ContattoInteressato.Nome + " " + ContattoInteressato.Cognome + " " + messaggio);
                            AggiornoLista();
                        }
                        riuscito = false;
                        
                    }
                    catch(Exception ex) //se c'è stato un errore nella lettura del messaggio si chiede di comunicare il rinvio
                    {
                        MessageBox.Show("Errore nell'arrivo del messaggio, chiedere a "+ContattoInteressato);
                        MessageBox.Show(ex.Message);
                    }


                }
            }
            
        }
        public void AggiornoLista()
        {
            //Con questo aggiorno la lista dei messaggi con i nuovi messaggi arrivati
            this.Dispatcher.Invoke(() =>
            {
                lstMessaggi.ItemsSource = "";
                lstMessaggi.ItemsSource = ListaMessaggi;
            });
           
        }
        public void CreaIndirizzoDiBroadcast()
        {
            //Genero un indirizzo che servira per fare il broadcast
            Contatto c = new Contatto("Broadcast", "", new IPEndPoint(0, 0));
            ListaContatti.Add(c);
            
            Contatti.ItemsSource = "";
            Contatti.ItemsSource = ListaContatti;
            Contatti.SelectedIndex = 0;
        }
        
        // Metodo eseguito quando l'utente clicca sul pulsante "Invia"
        private void btnInvia_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //mi salvo il contatto scelto
                
                    Contatto c = Contatti.SelectedItem as Contatto;
               
                   
                //se il contatto scelto è il broadcast mando il messaggio a tutti i miei contatti
                
                if(c.Nome== "Broadcast")
                {
                    for(int i=0;i<ListaContatti.Count;i++)
                    {
                        if(ListaContatti[i].Nome!= "Broadcast")
                        {
                            try
                            {
                                IPEndPoint remote_endpoint = ListaContatti[i].Indirizzo;
                                // Conversione del messaggio in un array di byte e invio del messaggio
                                byte[] messaggio = Encoding.UTF8.GetBytes(txtMessaggio.Text);
                                socket.SendTo(messaggio, remote_endpoint);
                            }catch(Exception ex)
                            {

                            }
                            
                        }
                        
                    }
                }else
                {
                    //se non è di broadcast lo mando singolarmente
                    IPEndPoint remote_endpoint = c.Indirizzo;
                    // Conversione del messaggio in un array di byte e invio del messaggio
                    byte[] messaggio = Encoding.UTF8.GetBytes(txtMessaggio.Text);
                    socket.SendTo(messaggio,0,messaggio.Length,SocketFlags.None, remote_endpoint);
                }
                
            }
            catch
            {
                MessageBox.Show("Indirizzo non valido"); //in caso di errore lo segnalo
            }
            
            
            
        }

        private void Aggiungi_Contatto_Click(object sender, RoutedEventArgs e)
        {
            //Con questo pulsante si aggiunge il contatti con i dati scelti nello xaml nella lista e quindi nella rubrica
            CreaContatto cc=new CreaContatto(ListaContatti);
            cc.ShowDialog();
            Contatti.ItemsSource = "";
            Contatti.ItemsSource = ListaContatti;
            Contatti.SelectedIndex = 0;
            SalvaContatti(ListaContatti);
        }
        public void SalvaContatti(List<Contatto> contatti, string NomeFile="Contatti.txt")
        {
            //salvo i contatti in un txt nella cartella del progetto chiamata contatti.txt che conterrà tutti i contatti per poi riuscire a riprenderli quando riapro il progetto
            List<string> ListaDeiContatti = new List<string>();

            foreach (Contatto c in contatti)
            {
                string contattoDaSalvare = c.Nome + ";" + c.Cognome + ";" + c.Indirizzo.ToString();
                ListaDeiContatti.Add(contattoDaSalvare);
            }

            File.WriteAllLines(NomeFile, ListaDeiContatti); //scrivo tutte le linee salvate nella lista

        }
        public void SalvaMessaggi(string NomeFile = "Messaggi.txt")
        {
            //salto tutti i messaggi per fare in modo che al riavvio li abbia di nuovo e lo faccio in un txt chiamato messaggi.txt
            List<string> ListaDeiMessaggi = new List<string>();

            foreach (string m in ListaMessaggi)
            {
                ListaDeiMessaggi.Add(m);
            }

            File.WriteAllLines(NomeFile, ListaDeiMessaggi); //scrivo nel file tutte le linee salvate nella lista
        }
        public void CaricaContatti(string NomeFile= "Contatti.txt")
        {
            //Con questo carico i contatti salvati nel txt

            string[] ListaContattiDaCaricare = File.ReadAllLines(NomeFile); //leggo le righe del file

            foreach (string ContattoDaCaricare in ListaContattiDaCaricare)
            {
                string[] Valori = ContattoDaCaricare.Split(';'); //le suddivido secondo le caratteristiche che avevo indicato quando li ho saltavi
                string[] ValoriIp=Valori[2].Split(':');
                if (Valori.Length >= 3)
                {
                    string nome = Valori[0];
                    string cognome = Valori[1];
                    IPAddress Indirizzo;
                    int Porta;
                    if (IPAddress.TryParse(ValoriIp[0], out Indirizzo) && int.TryParse(ValoriIp[1], out Porta))
                    {
                        if(nome!="Broadcast") //quello di broadcast è già presente di base quindi faccio in modo che non si duplichi
                        {
                            IPEndPoint indirizzo = new IPEndPoint(Indirizzo, Porta);
                            Contatto c = new Contatto(nome, cognome, indirizzo);
                            ListaContatti.Add(c); //se tutto va correttamente salvo il contatto
                        }
                        
                    }
                }
            }
            Contatti.ItemsSource = ""; //aggiorno la lista dei contatti
            Contatti.ItemsSource = ListaContatti;
            Contatti.SelectedIndex = 0;

        }
        public void CaricaMessaggi(string NomeFile = "Messaggi.txt")
        {

            //Carico dal txt tutti i messaggi
            string[] ListaMessaggiDaCaricare = File.ReadAllLines(NomeFile); //leggo tutti i messaggi

            foreach (string MessaggioDaCaricare in ListaMessaggiDaCaricare)
            {
                ListaMessaggi.Add(MessaggioDaCaricare); //aggiungo i messaggi alla lista
            }
            lstMessaggi.ItemsSource = ""; //Carico la lista nel listbox
            lstMessaggi.ItemsSource = ListaMessaggi;
            

        }
        private void EliminaContatto_Click(object sender, RoutedEventArgs e)
        {
            //elimino il contatto selezionato
            Contatto contatto = Contatti.SelectedItem as Contatto; //mi salvo quale contatto intende cancellare
            if (contatto.Nome!= "Broadcast") //non posso eliminare quello di broadcast
            {
                ListaContatti.Remove(contatto); //rimuovo il contatto e aggiorno la lista
                Contatti.ItemsSource = "";
                Contatti.ItemsSource = ListaContatti;
                Contatti.SelectedIndex = 0;
                SalvaContatti(ListaContatti); //aggiorno il txt
            }
            else
            {
                MessageBox.Show("Non puoi cancellare la chat di Broadcast");//indico l'errore nel provare a cancellare quello di broadcast
            }
        }

        private void PulisciMessaggi_Click(object sender, RoutedEventArgs e)
        {
            //cancello tutti i messaggi
            ListaMessaggi.Clear();
            AggiornoLista();
        }

        
    }
}
