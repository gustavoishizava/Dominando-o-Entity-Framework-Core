using System.Collections.Generic;

namespace Curso.Domain
{
    public class Departamento
    {
        public int Id { get; set; }

        public string Descricao { get; set; }

        public bool Ativo { get; set; }     

        public virtual List<Funcionario> Funcionarios { get; set; }
    }
}