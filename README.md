## TITOLO: Progettazione di database relazionali

# Schema concettuale dei dati:

- Progetto di un database;
- il modello E/R;
- concetto di entità, attributo, associazione;
- entità deboli;
- concetto di attributo chiave e attributo semplice;
- le associazioni tra entità: 1:1, 1:N, M:N;
- associazioni ricorsive;
- associazioni parziali;
- gerarchie IS_A, differenze tra: gerarchia totale e parziale; gerarchia
- esclusiva e sovrapposta

# Modelli logici dei dati: il modello relazionale

- caratteristiche e proprietà;
- concetto matematico di relazione;
- domini della relazione;
- grado della relazione;
- cardinalità della relazione;
- Strutture dei dati: relazioni, tuple, attributi, domini;
- Regole di mappaggio dal modello E/R al modello logico relazionale:
- pre-passi: Trasformazione delle gerarchie sovrapposte in gerarchie uniformi;
- Eliminazione delle gerarchie:
    - metodo1: accorpamento dei figli nella generalizzazione padre
    - metodo2: accorpamento del padre nei figli
    - metodo3: sostituzione della gerarchia con associazioni
- passo1: mappaggio entità forti
- passo2: mappaggio entità deboli
- passo3: mappaggio delle associazioni 1:1
- passo4: mappaggio delle associazioni 1:N
- passo5: mappaggio delle associazioni M:N
- passo6: trasformazione degli attributi multivalore

# Normalizzazione delle relazioni:

- le anomalie dei database: ridondanza dei dati, incongruenza e inconsistenza
- la normalizzazione: definizione e vantaggi
- concetto di dipendenza funzionale
- prima, seconda, terza forma normale, Boyce Codd normal form integrità dei dati: vincoli intrarelazionali e interrelazionali (integrità referenziale)

# Organizzazione logica dei file:

- Concetto di indice
- Area primaria e area degli indici e area delle inserzioni successive
- Indici densi e indici sparsi
- Indici primari e indici secondari
- Accesso per chiave primaria e secondaria, concetto di rottura di chiave Dal database logico a al database fisico: differenza tra record logico e record fisico



## TITOLO: Algebra Relazionale e linguaggio SQL

# Algebra relazionale: le operazioni relazionali:

- operatori algebrici:
    - taglio verticale: proiezione
    - taglio Orizzontale: selezione
    - Join: equi-join, left-join, right-join, full-join
- operatori insiemistici:
    - unione
    - intersezione
    - differenza

# Comandi per la definizione dei dati:

- creazione, modifica e cancellazione di tabelle ed indici.
- Istruzioni DDL: Create, Drop, Alter.

# Comandi per la manipolazione dei dati:

- Istruzioni DML: Insert Into, Update, Delete

# Comandi per la ricerca delle informazioni:

- Il comando Select con le clausole From, Where, As
- Gli operatori di Ricerca: Between, In, Like, IsNull
- Giunzione di tabelle con clausola Where o Inner Join.
- I Join esterni: Left Join e Right Join.
- Ordinamento dei dati tramite Order by
- Raggruppamento dei dati mediante Group by ed uso di funzioni di
- aggregazione: Count, Sum, Avg, Min, Max.
- la clausola Having
- I predicati IN, ANY, ALL, EXSIST
- Gli operatori insiemistici EXCEPT, INTERSECT
- Interrogazioni Annidate 

# TODO | Altri concetti SQL:

- Il concetto di vista: il comando CREATE VIEW
- I comandi per la sicurezza: GRANT E REVOKE
- I concetto di transazione: esempi
- la gestione delle transazioni con i comandi: BEGIN TRANSACTION, COMMIT E ROLL BACK



# TODO | TITOLO: Diritto e informatica

# TODO | La tutela della privacy:

- la normativa vigente;
- il Garante della privacy.
- GDPR

# TODO | Legge europea sull’utilizzo dei cookies

- La tutela del copyright:
- il diritto d’autore;
- il dibattito sulla brevettabilità del software;
- licenze d’uso e classificazione del software



## TITOLO: SQL SERVER

- Tipi di Dati
- Data Base Administration: gestione di tabelle, relazioni, viste
- Funzioni che restituiscono un recordset e funzioni scalari
- Stored Procedure con parametri in input e in output
- I Trigger
- Le Transazioni
- Il Merge tra database



## TITOLO: PROGRAMMAZONE LATO SERVER: ASP-NET

- Introduzione alla tecnologia ASP NET
- Invio dei dati di un modulo al Server tramite il pulsante di Submit
- Principali caratteristiche di ASP NET
- Separazione del codice dall’interfaccia: concetto di Form Code Behind
- Concetto di Web Form e relativi controlli server
- Accesso e condivisione di un database mediante il componente ADO NETL’oggetto View State
- Gestione degli eventi. L’attributo autopostback
- Principali controlli utilizzabili all’interno di una web form
- Sequenza di elaborazione di una web form
- Gestione dell’aspetto grafico di una Web Form mediante CSS ed il Framework
- Bootstrap;
- Principali oggetti ASP: Response, Request, Server, Session, ApplicationUtilizzo dei cookies
- I file Global.Asax e Web.Config
- Controlli server per la validazione dell’input
- Utilizzo framework Bootstrap



## TITOLO: WEB SERVICE RESTFul basati su MVC

- Approccio MVC per l’accesso ai dati e implementazione in ASPNet
- Concetto di Web Services: differenza tra SOAP e REST
- Chiamate asincrone lato client per il consumo di un service ASPNet
- Implementazione di Web Services API RESTFul in ASP NET
- Gestione degli accessi ad un’applicazione web tramite l’utilizzo di JWT



# TITOLO: ANALISI E REALIZZAZIONE DI SOLUZIONI MULTIPROGETTO

- Realizzazione di applicazioni web
- Realizzazione di web service REST e APIRESTFul
- Client HTML + JS di test per i Web Service di cui sopra


# domande

- Copyright e diritto d'autore ?
- open source che cos'è ?
- software semilibero ?
- associazioni ricorsive 
- che cos'è la normalizzazione
- i problemi di una base dati ??
- le regole di normalizzazione
- che cos'è l'algebra relazionale ??
- con operatori insiemistici ??
- come si implementa la differenza in sql ??
- come si implementa l'intersezione in sql ??
- web service RESTful cosa sono ??
- cosa sono REST e SOAP ??
- verbi usati in HTTP dai REST ??
- variabile di sessione che cos'è ??
- variabile di applicazione che cos'è ??
- come si imposta una variabile di sessione in un linguaggio qualsiasi, come viene definita e dove ??
- cosa viene allocato nella memoria di massa sul dispositivo (cookie)
- che cos'è un cookie ???
- copyright-left ??
- top 10 tipi di software e differenze ??

- Copyright e diritto d'autore

Il copyright è un titolo che consente a chi lo detiene di distribuire, modificare e sfruttare a scopo di lucro un prodotto, nel nostro caso software.
Quando si acquista un software di solito si ottiene la licenza d'uso, un contratto che specifica i termini di utilizzo del prodotto. Questi possono variare da
software freeware a software proprietario.
Il diritto d'autore si divide in diritto morale, di paternità dell'opera, che non decade mai, e diritto patrimoniale, ossia diritto all'uso dell'opera, 
che decade a 70 anni dopo la morte.

Lista dei software:
SOFTWARE LIBERO - libertà totale, quindi di modifica, distribuzione, e guadagno, ma non sempre gratuitamente.
COPYLEFT (con permesso d'autore) - stesse libertà del freeware ma deve essere sempre distribuito come freeware, senza la possibilità di porre restrizioni all'uso.
DOMINIO PUBBLICO (senza permesso d'autore) - alcune riproduzioni del software possono avere restrizioni
SEMILIBERO - non posso ridistribuirlo a scopo di lucro ma posso modificarlo
FREEWARE - posso utilizzarlo e ridistribuirlo, ma non ho accesso al codice sorgente
SHAREWARE - di solito non è disponibile il codice sorgente, e per ridistribuirlo devo pagare una licenza
PROPRIETARIO - utilizzo, copia, modifica e distribuzione sono vietati o richiedono permessi.
COMMERCIALE - creato da aziende a scopo di lucro, può rientrare nelle altre categorie

- open source che cos'è

Software del quale ho accesso al codice sorgente

- software semilibero

non posso ridistribuirlo a scopo di lucro ma posso modificarlo

- associazioni ricorsive [DOPO]
- che cos'è la normalizzazione
- i problemi di una base dati ??
- le regole di normalizzazione
- che cos'è l'algebra relazionale ??
- con operatori insiemistici ??
- come si implementa la differenza in sql ??
- come si implementa l'intersezione in sql ??
- web service RESTful cosa sono ??
- cosa sono REST e SOAP ??
- verbi usati in HTTP dai REST ??
- variabile di sessione che cos'è ??
- variabile di applicazione che cos'è ??
- come si imposta una variabile di sessione in un linguaggio qualsiasi, come viene definita e dove ??
- cosa viene allocato nella memoria di massa sul dispositivo (cookie)
- che cos'è un cookie ???
- copyright-left ??
- top 10 tipi di software e differenze ??
