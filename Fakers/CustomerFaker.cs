﻿
namespace SampleMauiMvvmApp.Fakers
{
    // https://github.com/bchavez/Bogus
    public class CustomerFaker : Faker<Customer>
    {
        public CustomerFaker()
        {
            //RuleFor(x => x.Name, x => x.Company.CompanyName());
            //RuleFor(x => x.Abn, x => x.Finance.CreditCardNumber());
            //RuleFor(x => x.Phone, x => x.Person.Phone);
            //RuleFor(x => x.Email, x => x.Person.Email);
            //RuleFor(x => x.Address, x => x.Address.FullAddress());
        }
    }
}
