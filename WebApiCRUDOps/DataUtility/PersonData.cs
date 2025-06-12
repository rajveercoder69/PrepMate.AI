using Microsoft.AspNetCore.Http.HttpResults;
using WebApiCRUDOps.DataUtility.Model;

namespace WebApiCRUDOps.DataUtility
{
    public class PersonData
    {
        public static List<Person> shirts = new List<Person>()
        { 
             new Person{personId=0,firstName="blue",lastName="Arrow",gender="men",price=49.99,size=28 },
            
             new Person{personId=2,firstName="paleblue",lastName="PeterEngland",gender="men",price=119.99,size=38 },
 
             new Person{personId=3,firstName="marron",lastName="Jameshamstephedan",gender="men",price=49.99,size=42 },

             new Person{personId=4,firstName="BabyPinkishBlue",lastName="Zara",gender="women",price=149.99,size=28 }

        };
        public static Person CheckPersonId(int id)
        {
            var personToUpdate = shirts.First(x => x.personId == id);
            if (personToUpdate==null)
            {
                return null;
            }
            return personToUpdate;
        }
        //public static Shirt UpdateShirtdetails(Shirt prevShirt,Shirt nextShirt)
        //{
        //    var shirtToUpdate = prevShirt;
        //    shirtToUpdate.price=nextshirt.price;
        //    shirtToUpdate.gender=shirt.gender;
        //    shirtToUpdate.size=shirt.size;
        //    shirtToUpdate.brandName=shirt.brandName;
        //    shirtToUpdate.color=shirt.color;
        //    return shirtToUpdate;
        //}
        public static void AddPersont(Person shirt)
        {
            int maxShirtId=shirts.Max(x=>x.personId);
            shirt.personId=maxShirtId+1;
            shirts.Add(shirt);
        }
        public static void DeletPerson(int id)
        {
            var personToDelete = shirts.FirstOrDefault(x => x.personId == id);
            shirts.Remove(personToDelete);
        }
    }
}
