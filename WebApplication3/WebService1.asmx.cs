using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Xml.Serialization;

namespace WebApplication3
{
    /// <summary>
    /// Description résumée de WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    

    // Pour autoriser l'appel de ce service Web depuis un script à l'aide d'ASP.NET AJAX, supprimez les marques de commentaire de la ligne suivante. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        private interfaceReservation ir;
        private List<Agence> compte= new List<Agence>();
        private Dictionary<int, List<int>> Reser = new Dictionary<int, List<int>>();
        public WebService1()
        {
            List<IChambre> chambres = new List<IChambre>();
            List<IChambre> chambres1 = new List<IChambre>();
            List<Reservation> resas = new List<Reservation>();
            for (int i = 0; i < 10; i++)
            {
                chambres.Add(new ChambreDouble(new Chambre("chambredouble1.jpg",1, 56, i)));
                chambres1.Add(new ChambreDouble(new Chambre("chambredouble2.jpg",1, 70, i)));
            }

            Console.WriteLine(chambres.ElementAt(0).getPrixParNuitTTC());

            for (int i = 0; i < 10; i++)
            {
                chambres.Add(new ChambreSimple(new Chambre("chambres1.jpg",1, 56, i)));
                chambres1.Add(new ChambreSimple(new Chambre("chambres2.jpg", 1, 70, i)));
            }

            Hotel hotel = new Hotel(4, "Montpellier", "31 rue de la mediterranee", "Le mediterraneen", 5, chambres);
            Hotel hotel1 = new Hotel(4, "Montpellier", "gare saint roch", "Campanile", 5, chambres1);

            List<Hotel> hotels = new List<Hotel>();
            hotels.Add(hotel);
            hotels.Add(hotel1);
             ir = new interfaceReservation(hotels);
            if (Context.Cache.Get("compte") != null)
            {
                this.compte = (List<Agence>)Context.Cache.Get("compte");
            } 
            if (Context.Cache.Get("ir") != null)
            {
                this.ir = (interfaceReservation)Context.Cache.Get("ir");
            } 
            if (Context.Cache.Get("dico") != null)
            {
                this.Reser = (Dictionary<int, List<int>>)Context.Cache.Get("dico");
            }
        }
        [WebMethod]
        public interfaceReservation getIr()
        {
            return ir;
        }
        [WebMethod]
        public Reservation getresr()
        {
            if (Context.Cache.Get("ir") != null)
            {
                this.ir = (interfaceReservation)Context.Cache.Get("ir");
            }
            return ir.hotels.ElementAt(0).chambres.ElementAt(0).getResr().ElementAt(0);
        }
        [WebMethod]
        [XmlInclude(typeof(Agence))]
        public List<String> RechercherIdHotel(string ville,int nbEtoiles)
        {
            List<String> resultat = new List<String>();
            int x = 0;
            foreach(Hotel h in ir.hotels)
            {
                if(h.getVille().Equals(ville) && h.getNbEtoiles() == nbEtoiles)
                {
                    resultat.Add(x + " : " + h.getNom() +" "+ nbEtoiles+" Etoiles");
                }
                x += 1;
            }
            return resultat;
        }
        [WebMethod]
        [XmlInclude(typeof(Agence))]
        public void signUp(String Identifiant,String motDePasse) 
        {
            compte.Add(new Agence(Identifiant, motDePasse));
            Context.Cache.Insert("compte", compte);
        }

   



        [WebMethod]
        [XmlInclude(typeof(ChambreDouble))]
        [XmlInclude(typeof(ChambreSimple))]
        [XmlInclude(typeof(Chambre))]
        [XmlInclude(typeof(Agence))]
        public List<String> Rechercher(String ville, DateTime arrivee, DateTime depart, int prixMin, int prixMax, int nbEtoiles, int nbDePersonnes,String identifiant,String Motdepasse, int i)
        {
            List<String> res = new List<String>();
            if (Context.Cache.Get("ir") != null)
            {
                this.ir = (interfaceReservation)Context.Cache.Get("ir");
            }
            Agence b = new Agence();
            if (this.compte != null)
            {


                foreach (Agence a in compte)
                {
                    if (a.getID().Equals(identifiant))
                    {
                        if (a.getMDP().Equals(Motdepasse))
                        {
                            b = a;
                        }
                    }
                }
            }
            if (b.getID() == null)
            {
                res.Add("Utilisateur inconnue pensez a crée une agence");
                return res;
            }
            Dictionary<string, List<int>> resultatRecherche = ir.rechercher(ville, arrivee, depart, prixMin, prixMax, nbEtoiles, nbDePersonnes, b, i);
            Random aleatoire = new Random();
            int x;
            foreach (KeyValuePair<String, List<int>> kvp in resultatRecherche)
            {
                
                x = aleatoire.Next(1111111, 9999999);
                res.Add(  x +"  "+ (String)kvp.Key);
                while (Reser.ContainsKey(x))
                {
                    x = aleatoire.Next(1111111, 9999999);
                }
                Reser.Add(x, kvp.Value);
                Context.Cache.Insert("dico", Reser);
            }

            return res ;
            
        }
        [WebMethod]
        [XmlInclude(typeof(ChambreDouble))]
        [XmlInclude(typeof(ChambreSimple))]
        [XmlInclude(typeof(Chambre))]
        [XmlInclude(typeof(Agence))]
        public String reserFinal(int x,int id,DateTime arr,DateTime fin,String nom,String prenom)
        {
         
            if (Reser.ContainsKey(x))
            {
                ir.hotels.ElementAt(id).confirmerRes(Reser[x], arr,fin,nom,prenom);
                Context.Cache.Insert("ir", ir);
                return "Votre réservation a été effectué avec succés";
            }
            return "Votre id est incorrect ";
            
        }
        [WebMethod]
        [XmlInclude(typeof(IChambre))]
        public byte[] GetImageFile(int a,int b)
        {
            if (System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/TransientStorage/") + ir.hotels.ElementAt(a).chambres.ElementAt(b).getImagePath()))
                return System.IO.File.ReadAllBytes(System.Web.Hosting.HostingEnvironment.MapPath("~/TransientStorage/") + ir.hotels.ElementAt(a).chambres.ElementAt(b).getImagePath());
            else
                return new byte[] { 0 };
        }
    }
   

    public class interfaceReservation
    {
        public List<Hotel> hotels = new List<Hotel>();
         public interfaceReservation()
        {

        }
        public interfaceReservation(List<Hotel> h)
        {
            this.hotels = h;
        }

        public Dictionary<string, List<int>> rechercher(String ville, DateTime arrivee, DateTime depart, int prixMin, int prixMax, int nbEtoiles, int nbDePersonnes,Agence a, int i)
        {            
            Hotel h = hotels.ElementAt(i);
            
    
            Dictionary<string, List<int>> dico = new Dictionary<string, List<int>>();




            if (h.getVille().Equals(ville) && h.getNbEtoiles() == nbEtoiles && h.getPrixSejourChambreDouble(nbDePersonnes, arrivee, depart,a) < prixMax
                    && h.getPrixSejourChambreDouble(nbDePersonnes, arrivee, depart,a) > prixMin && (h.resrLibreDouble(nbDePersonnes, arrivee, depart)||h.resrLibreSimple(nbDePersonnes, arrivee, depart)))
                {
                 if (h.resrLibreDouble(nbDePersonnes, arrivee, depart))
                 {
                    
                    dico.Add( ":" + h.getNom() + " au prix Ttc : " + h.getPrixSejourChambreDouble(nbDePersonnes, arrivee, depart, a) + "Euros ("+nbDePersonnes/2+"chambre double et "+ nbDePersonnes%2 +"Chambre Simple)", h.reserver(arrivee, depart, nbDePersonnes));
                    
                    
                    
                 }
                 if (h.resrLibreSimple(nbDePersonnes, arrivee, depart))
                 {

                    dico.Add( ":" + h.getNom() + " au prix Ttc : " + h.getPrixSejourChambreSimple(nbDePersonnes, arrivee, depart, a) + "Euros ("+ nbDePersonnes +"Chambre Simple)", h.reserverSimple(arrivee, depart, nbDePersonnes));
                  
                   
                }


                }
            
           
            return dico;
        }
        public void reserver(Hotel h, DateTime arrivee, DateTime depart, int nbDePersonnes)
        {
            h.reserver(arrivee, depart, nbDePersonnes);

        }
        

    }


    public class Reservation
    {
        String nom;
        String Prenom;
        DateTime startDate;
        DateTime endDate;
        List<IChambre> c;
        Hotel h;
        int nbdeJours;
        public Reservation()
        {

        }
        public Reservation(DateTime startDate, DateTime endDate, List<IChambre> c, Hotel h)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            this.c = c;
            this.h = h;
        }
        public Reservation(DateTime startDate, DateTime endDate, Hotel h,String nom, String prenom)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            this.h = h;
            this.nom = nom;
            this.Prenom = prenom;

        }
        public DateTime getStartDate()
        {
            return this.startDate;
        }
        public DateTime getEndDate()
        {
            return this.endDate;
        }
        public Double getLongueurResa()
        {
            return (endDate.Date - startDate.Date).TotalDays;
        }




    }
    public class Agence
    {
        private String identifiant;
        private String motDePasse;
        public double taux;
        public Agence()
        {

        }
        public Agence(String a,String b)
        {
            this.identifiant = a;
            this.motDePasse = b;
            this.taux = 0.1;

        }
        public string getID()
        {
            return this.identifiant;
        }
        public string getMDP()
        {
            return this.motDePasse;
        }

        public Double getTaux()
        {
            return this.taux;
        }
    }
    public class Hotel
    {
       public  int nbDeChambres;
        public String lieu;
        public String adresse;
        public String nom;
        public int nbEtoiles;
        public List<IChambre> chambres = new List<IChambre>();
        public Agence agence;
        public Hotel()
        {

        }
        public Hotel(int nbDeChambres, String lieu, String adresse, String nom, int nbEtoiles)
        {
            this.nbDeChambres = nbDeChambres;
            this.lieu = lieu;
            this.adresse = adresse;
            this.nom = nom;
            this.nbEtoiles = nbEtoiles;
        }
        public Hotel(int nbDeChambres, String lieu, String adresse, String nom, int nbEtoiles, List<IChambre> c)
        {
            this.nbDeChambres = nbDeChambres;
            this.lieu = lieu;
            this.nom = nom;
            this.adresse = adresse;
            this.nbEtoiles = nbEtoiles;
            this.chambres = c;
        }


        public String getNom()
        {
            return nom;
        }
        public List<IChambre> getChambres()
        {
            return this.chambres;
        }
        public int getNbdeChambres()
        {
            return chambres.Count;
        }

        public List<int> reserver(DateTime arrivee, DateTime depart, int nbDePersonnes)
        {
            int nbChambreDouble = nbDePersonnes / 2;
            int nbChambreSimple = nbDePersonnes % 2;
            int x = 0; //nb de chambre dispo
            int y = 0;
            List<int> cham= new List<int>();

            for(int i=0;i<chambres.Count;i++)
            {
                if (chambres.ElementAt(i).getNbDePersonnes() == 2)
                {
                    if (chambres.ElementAt(i).chambreLibre(arrivee, depart) && x < nbChambreDouble)
                    {
                        
                        x += 1;
                        cham.Add(i);

                    }
                }

                if (chambres.ElementAt(i).getNbDePersonnes() == 1)
                {
                    if (chambres.ElementAt(i).chambreLibre(arrivee, depart) && x < nbChambreSimple)
                    {
                       
                        y += 1;
                        cham.Add(i);
                        
                    }
                }
                


            }
            return cham;
        }
        public List<int> reserverSimple(DateTime arrivee, DateTime depart, int nbDePersonnes)
        {
            
            int nbChambreSimple = nbDePersonnes;
            int x = 0; //nb de chambre dispo
            List<int> cham = new List<int>();

            for (int i = 0; i < chambres.Count; i++)
            {
                

                if (chambres.ElementAt(i).getNbDePersonnes() == 1)
                {
                    if (chambres.ElementAt(i).chambreLibre(arrivee, depart) && x < nbChambreSimple)
                    {

                        x += 1;
                        cham.Add(i);

                    }
                }



            }
            return cham;
        }
        public void confirmerRes(List<int> c, DateTime arrivee, DateTime depart,String nom,String prenom)
        {
            foreach(int ch in c)
            {
                chambres.ElementAt(ch).addReservation(new Reservation(arrivee, depart, this,nom,prenom));
            }
        }

        public Boolean resrLibreSimple(int nbdePersonnes, DateTime startDate, DateTime endDate)
        {
            int y = 0;
            foreach (IChambre ch in chambres)
            {
                if (ch.getNbDePersonnes() == 1)
                {
                 if (ch.chambreLibre(startDate, endDate))
                 {
                    y += 1;

                 }

                }

            }
            return y >= nbdePersonnes;
        }

            public Boolean resrLibreDouble(int nbdePersonnes, DateTime startDate, DateTime endDate)
        {
            int nbChambreDouble = nbdePersonnes / 2;
            int nbChambreSimple = nbdePersonnes % 2;
            int x = 0; //nb de chambre dispo
            int y = 0;
            foreach (IChambre ch in chambres)
            {
                if (ch.getNbDePersonnes() == 2)
                {
                    if (ch.chambreLibre(startDate, endDate))
                    {
                        x += 1;

                    }

                }
                if (ch.getNbDePersonnes() == 1)
                {
                    if (ch.chambreLibre(startDate, endDate))
                    {
                        y += 1;

                    }

                }

            }
            return x >= nbChambreDouble && y >= nbChambreSimple;
        }


        public Double getPrixSejourChambreDouble(int nbdePersonnes, DateTime startDate, DateTime endDate,Agence a)
        {

            int duree = (int)(endDate.Date - startDate.Date).TotalDays;
            int prixTTC = 0;

            int nbChambreDouble = nbdePersonnes / 2;
            int nbChambreSimple = nbdePersonnes % 2;
            for (int i = 0; i < nbChambreDouble; i++)
            {
                foreach (IChambre ch in chambres)
                {
                    if (ch.getNbDePersonnes() == 2)
                    {
                        prixTTC += ch.getPrixParNuitTTC();
                        break;
                    }

                }
            }
            for (int i = 0; i < nbChambreSimple; i++)
            {
                foreach (IChambre ch in chambres)
                {
                    if (ch.getNbDePersonnes() == 1)
                    {
                        prixTTC += ch.getPrixParNuitTTC();
                        break;
                    }
                }
            }
            return (prixTTC* duree)-(prixTTC *duree)*a.getTaux();
        }
        public Double getPrixSejourChambreSimple(int nbdePersonnes, DateTime startDate, DateTime endDate, Agence a)
        {

            int duree = (int)(endDate.Date - startDate.Date).TotalDays;
            int prixTTC = 0;


            int nbChambreSimple = nbdePersonnes;
           
            for (int i = 0; i < nbChambreSimple; i++)
            {
                foreach (IChambre ch in chambres)
                {
                    if (ch.getNbDePersonnes() == 1)
                    {
                        prixTTC += ch.getPrixParNuitTTC();
                        break;
                    }
                }
            }
            return (prixTTC * duree) - (prixTTC * duree) * a.getTaux();
        }

        public String getVille()
        {
            return this.lieu;
        }
        public int getNbEtoiles()
        {
            return this.nbEtoiles;
        }




    }

    public abstract class IChambre
    {

        public abstract int getNbDePersonnes();

        public abstract int getNumero();
        public abstract Boolean chambreLibre(DateTime start, DateTime end);

        public abstract int getPrixParNuitTTC();
        public abstract List<Reservation> getResr();

        [WebMethod]
        public abstract String getImagePath();
        public abstract void addReservation(Reservation r);


    }

   public class Chambre : IChambre
    {
        public int nbDePersonnes;
        public string imagePath;
        public int prixParNuitTTC;
        public int numero;
        public List<Reservation> reservations = new List<Reservation>();

        public Chambre()
        {

        }
        public Chambre(string imagePath,int nbDePersonnes, int prixParNuitTTC, int numero)
        {
            this.nbDePersonnes = nbDePersonnes;
            this.imagePath = imagePath;
            this.prixParNuitTTC = prixParNuitTTC;
            this.numero = numero;
        }
        public override Boolean chambreLibre(DateTime start, DateTime end)
        {
            
            Boolean b = true;
            if (reservations != null)
            {
                foreach (Reservation r in reservations)
                {
                    if (r.getStartDate() < start)
                    {

                        if (r.getEndDate() > start)
                        {
                            b = false;
                        }
                    }
                    else if (r.getStartDate() < end)
                    {
                        b = false;
                    }
                }
            }
            return b;
        }

        public override string getImagePath()
        {
            return this.imagePath;
        }

        public override int getNumero()
        {
            return this.numero;
        }

        public override int getPrixParNuitTTC()
        {
            return this.prixParNuitTTC;
        }

        public override int getNbDePersonnes()
        {
            return this.nbDePersonnes;
        }

        public override void addReservation(Reservation r)
        {
            reservations.Add(r);
        }

        public override List<Reservation> getResr()
        {
           return reservations;
        }
    }

    public class ChambreDecorateur : IChambre
    {
        private IChambre _decore;

        public ChambreDecorateur()
        {
        }

        public ChambreDecorateur(IChambre c)
        {
            this._decore = c;
        }

        public override void addReservation(Reservation r)
        {
            _decore.addReservation(r);
        }

        public override bool chambreLibre(DateTime start, DateTime end)
        {
            return _decore.chambreLibre(start, end);
        }

        public override string getImagePath()
        {
            return _decore.getImagePath();
        }

        public override int getNbDePersonnes()
        {
            return this._decore.getNbDePersonnes();

        }

        public override int getNumero()
        {
            return this._decore.getNumero();
        }

        public override int getPrixParNuitTTC()
        {
            return this._decore.getPrixParNuitTTC();
        }

        public override List<Reservation> getResr()
        {
            return _decore.getResr();
        }
    }
    public class ChambreSimple : ChambreDecorateur
    {
        public int nbDePersonnes;
        public int prixParNuitTTC;
        public ChambreSimple() : base()
        {

        }
        public ChambreSimple(IChambre c) : base(c)
        {
            this.nbDePersonnes = base.getNbDePersonnes();
            this.prixParNuitTTC = base.getPrixParNuitTTC();

        }

        public override int getNumero()
        {
            return base.getNumero();
        }

        public override int getPrixParNuitTTC()
        {
            return base.getPrixParNuitTTC();
        }

        public override int getNbDePersonnes()
        {
            return base.getNbDePersonnes();
        }


    }

   public class ChambreDouble : ChambreDecorateur
    {
        public int nbDePersonnes;
        public int prixParNuitTTC;

        public ChambreDouble() : base()
        {

        }
        public ChambreDouble(IChambre c) : base(c)
        {
            nbDePersonnes = base.getNbDePersonnes() * 2;
            prixParNuitTTC = base.getPrixParNuitTTC() * 2-20;
        }

        public override int getNumero()
        {
            return base.getNumero();
        }

        public override int getPrixParNuitTTC()
        {
            return this.prixParNuitTTC;
        }

        public override int getNbDePersonnes()
        {
            return this.nbDePersonnes;
        }

    }
}
