//Per fare il debug di questi esempi, devi prima installare il global tool dotnet-script con questo comando:
//dotnet tool install -g dotnet-script
//Trovi altre istruzioni nel file /scripts/readme.md


//ESEMPIO #1: Definisco una lambda che accetta un parametro DateTime e restituisce un bool, e l'assegno alla variabile canDrive
Func<DateTime, bool> canDrive = dob => {
    return dob.AddYears(18) <= DateTime.Today;
};

//Eseguo la lambda passandole il parametro DateTime
DateTime dob = new DateTime(2002, 12, 25);
bool result = canDrive(dob);
//Poi stampo il risultato bool che ha restituito
Console.WriteLine(result);

//ESEMPIO #2: Stavolta definisco una lambda che accetta un parametro DateTime ma non restituisce nulla
Action<DateTime> printDate = (date) => Console.WriteLine(date);

//La invoco passandole l'argomento DateTime
DateTime date = DateTime.Today;
printDate(date);

/*** ESERCIZI! ***/

// ESERCIZIO #1: Scrivi una lambda che prende due parametri stringa (nome e cognome) e restituisce la loro concatenazione
   Func<string,string,string> concatFirstAndLastName = (nome, cognome) => {return $"{nome} {cognome}";};
   Console.WriteLine(concatFirstAndLastName("Marco","Albasini"));

// ESERCIZIO #2: Una lambda che prende tre parametri interi (tre numeri) e restituisce il maggiore dei tre
   Func<int,int,int,int> getMaximum = (num1,num2,num3)=> {return Math.Max(Math.Max(num1,num2),num3);};
   Console.WriteLine(getMaximum(5,9,67));

// ESERCIZIO #3: Una lambda che prende due parametri DateTime e non restituisce nulla, ma stampa la minore delle due date in console con un Console.WriteLine
   Action<DateTime,DateTime> printLowerDate = (data1,data2)=>{
       if(data1<data2)
       {
        Console.WriteLine(data1);
       }
       else
       {
        Console.WriteLine(data2);
       }

   };
   printLowerDate(new DateTime(2020,05,01), DateTime.Today);