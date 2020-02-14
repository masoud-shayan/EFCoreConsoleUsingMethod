using System;

namespace WorkingWithEFCore
{
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int YearsOfExperience  { get; set; }
        public DateTime Birthday { get; set; }

        public string FullName
        {
            get { return $"{FirstName} {LastName}"; }
        }
    }
}