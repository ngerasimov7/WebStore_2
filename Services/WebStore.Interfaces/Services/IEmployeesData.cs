using System.Collections.Generic;
using WebStore.Domain.Models;

namespace WebStore.Interfaces.Servcies
{
    public interface IEmployeesData
    {
        IEnumerable<Employee> Get();

        Employee Get(int id);

        Employee GetByName(string LastName, string FirstName, string Patronymic);

        int Add(Employee employee);

        Employee Add(string LastName, string FirstName, string Patronymic, int Age);

        void Update(Employee employee);

        bool Delete(int id);
    }
}