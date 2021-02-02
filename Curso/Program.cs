using System.Data;
using System.Diagnostics;
using System;
using Curso.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;
using Curso.Domain;

namespace DominandoEFCore
{
    class Program
    {
        static void Main(string[] args)
        {
            // EnsureCreatedAndDeleted();
            // GapDoEnsureCreated();
            // HealthCheckBancoDeDados();

            // warmup
            // new ApplicationContext().Departamentos.AsNoTracking().Any();
            // _count = 0;
            // GerenciarEstadoDaConexao(false);
            // _count = 0;
            // GerenciarEstadoDaConexao(true);
            // SqlInjection();
            // MigracoesPendentes();
            // AplicarMigracaoEmtempoExecucao();
            // TodasMigracoes();
            // MigracaoJaAplicadas();
            // ScriptGeralDoBancoDeDados();
            // CarregamentoAdiantado();
            // CarregamentoExplicito();
            // CarregamentoLento();

            // FiltroGlobal();
            // IgnorandoFiltroGlobal();
            // ConsultaProjetada();
            // ConsultaParametrizada();
            // ConsultaInterpolada();
            // ConsultaComTag();
            // EntendendoConsulta1NN1();
            DivisaoDeConsulta();
        }

        static void EnsureCreatedAndDeleted()
        {
            using var db = new ApplicationContext();
            // db.Database.EnsureCreated();
            db.Database.EnsureDeleted();
        }

        static void GapDoEnsureCreated()
        {
            using var db1 = new ApplicationContext();
            using var db2 = new ApplicationContextCidade();

            db1.Database.EnsureCreated();
            db2.Database.EnsureCreated();

            var databaseCreator = db2.GetService<IRelationalDatabaseCreator>();
            databaseCreator.CreateTables();
        }

        static void HealthCheckBancoDeDados()
        {
            using var db = new ApplicationContext();
            var canConnection = db.Database.CanConnect();

            if (canConnection)
            {
                Console.WriteLine("Posso me conectar.");
            }
            else
            {
                Console.Write("Não posso me conectar.");
            }
        }

        static int _count;
        static void GerenciarEstadoDaConexao(bool gerenciarEstadoConexao)
        {
            using var db = new ApplicationContext();
            var time = Stopwatch.StartNew();

            var conexao = db.Database.GetDbConnection();
            conexao.StateChange += (_, __) => ++_count;
            if (gerenciarEstadoConexao)
            {
                conexao.Open();
            }

            for (var i = 0; i < 200; i++)
            {
                db.Departamentos.AsNoTracking().Any();
            }

            time.Stop();
            var mensagem = $"Tempo : {time.Elapsed.ToString()}. Gerenciando estado: {gerenciarEstadoConexao}. Contador: {_count} \n";
            Console.Write(mensagem);
        }

        static void ExecuteSQL()
        {
            using var db = new ApplicationContext();

            // Primeira opção
            using (var cmd = db.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = "SELECT 1";
                cmd.ExecuteNonQuery();
            }

            // Segunda opção
            var descricao = "Teste";
            db.Database.ExecuteSqlRaw("UPDATE Departamentos SET Descricao={0} WHERE Id=1", descricao);

            // Terceira opção
            db.Database.ExecuteSqlInterpolated($"UPDATE Departamentos SET Descricao={descricao} WHERE Id=1");
        }

        static void SqlInjection()
        {
            using var db = new ApplicationContext();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Departamentos.AddRange(
                new Departamento
                {
                    Descricao = "Departamento 01"
                },
                new Departamento
                {
                    Descricao = "Departamento 02"
                }
            );

            db.SaveChanges();

            var descricao = "Departamento 01";
            // Maneira correta
            // db.Database.ExecuteSqlRaw("UPDATE Departamentos SET Descricao='DepartamentoAlterado' WHERE Descricao={0}", descricao);

            // Maneira incorreta
            descricao = "Teste ' or 1='1";
            db.Database.ExecuteSqlRaw($"UPDATE Departamentos SET Descricao='Ataque SQL Injection' WHERE Descricao='{descricao}'");

            foreach (var departamento in db.Departamentos.AsNoTracking())
            {
                Console.WriteLine($"Id: {departamento.Id}, Descrição: {departamento.Descricao}");
            }
        }

        static void MigracoesPendentes()
        {
            using var db = new ApplicationContext();

            var migracoesPendentes = db.Database.GetPendingMigrations();

            Console.WriteLine($"Total: {migracoesPendentes.Count()}");

            foreach (var migracao in migracoesPendentes)
            {
                Console.WriteLine($"Migracao: {migracao}");
            }
        }

        static void AplicarMigracaoEmtempoExecucao()
        {
            using var db = new ApplicationContext();
            db.Database.Migrate();
        }

        static void TodasMigracoes()
        {
            using var db = new ApplicationContext();

            var migracoes = db.Database.GetMigrations();

            Console.WriteLine($"Total: {migracoes.Count()}");

            foreach (var migracao in migracoes)
            {
                Console.WriteLine($"Migracao: {migracao}");
            }
        }

        static void MigracaoJaAplicadas()
        {
            using var db = new ApplicationContext();

            var migracoes = db.Database.GetAppliedMigrations();

            Console.WriteLine($"Total: {migracoes.Count()}");

            foreach (var migracao in migracoes)
            {
                Console.WriteLine($"Migracao: {migracao}");
            }
        }

        static void ScriptGeralDoBancoDeDados()
        {
            using var db = new ApplicationContext();
            var script = db.Database.GenerateCreateScript();

            Console.WriteLine(script);
        }

        static void CarregamentoAdiantado()
        {
            using var db = new ApplicationContext();
            SetupTiposCarregamentos(db);

            var departamentos = db.Departamentos.Include(x => x.Funcionarios);

            foreach (var departamento in departamentos)
            {
                Console.WriteLine("------------------------------------");
                Console.WriteLine($"Departamento: {departamento.Descricao}");

                if (departamento.Funcionarios?.Any() ?? false)
                {
                    foreach (var funcionario in departamento.Funcionarios)
                    {
                        Console.WriteLine($"\nFuncionário: {funcionario.Nome}");
                    }
                }
                else
                {
                    Console.WriteLine("\nNenhum funcionário encontrado!");
                }
            }
        }

        static void CarregamentoExplicito()
        {
            using var db = new ApplicationContext();
            SetupTiposCarregamentos(db);

            var departamentos = db.Departamentos.ToList();

            foreach (var departamento in departamentos)
            {
                if (departamento.Id == 2)
                {
                    // db.Entry(departamento).Collection(x => x.Funcionarios).Load();
                    db.Entry(departamento).Collection(x => x.Funcionarios).Query().Where(x => x.Id > 2).ToList();

                }

                Console.WriteLine("------------------------------------");
                Console.WriteLine($"Departamento: {departamento.Descricao}");

                if (departamento.Funcionarios?.Any() ?? false)
                {
                    foreach (var funcionario in departamento.Funcionarios)
                    {
                        Console.WriteLine($"\nFuncionário: {funcionario.Nome}");
                    }
                }
                else
                {
                    Console.WriteLine("\nNenhum funcionário encontrado!");
                }
            }
        }

        static void CarregamentoLento()
        {
            using var db = new ApplicationContext();
            SetupTiposCarregamentos(db);

            // db.ChangeTracker.LazyLoadingEnabled = false;

            var departamentos = db.Departamentos.ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine("------------------------------------");
                Console.WriteLine($"Departamento: {departamento.Descricao}");

                if (departamento.Funcionarios?.Any() ?? false)
                {
                    foreach (var funcionario in departamento.Funcionarios)
                    {
                        Console.WriteLine($"\nFuncionário: {funcionario.Nome}");
                    }
                }
                else
                {
                    Console.WriteLine("\nNenhum funcionário encontrado!");
                }
            }
        }

        static void SetupTiposCarregamentos(ApplicationContext db)
        {
            if (!db.Departamentos.Any())
            {
                db.Departamentos.AddRange(
                    new Departamento
                    {
                        Ativo = true,
                        Descricao = "Departamento 01",
                        Funcionarios = new System.Collections.Generic.List<Funcionario>{
                            new Funcionario{
                                CPF = "00",
                                RG = "00",
                                Nome = "Gustavo"
                            }
                        }
                    },
                     new Departamento
                     {
                         Ativo = true,
                         Descricao = "Departamento 02",
                         Funcionarios = new System.Collections.Generic.List<Funcionario>{
                            new Funcionario{
                                CPF = "00",
                                RG = "00",
                                Nome = "Vitória"
                            }
                        }
                     }
                );

                db.SaveChanges();
                db.ChangeTracker.Clear();
            }
        }

        static void FiltroGlobal()
        {

            using var db = new ApplicationContext();
            Setup(db);

            var departamentos = db.Departamentos.Where(x => x.Id > 0).ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao} \t Excluído: {departamento.Excluido}");
            }
        }

        static void IgnorandoFiltroGlobal()
        {

            using var db = new ApplicationContext();
            Setup(db);

            var departamentos = db.Departamentos.IgnoreQueryFilters().Where(x => x.Id > 0).ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao} \t Excluído: {departamento.Excluido}");
            }
        }

        static void ConsultaProjetada()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var departamentos = db.Departamentos
                .Where(x => x.Id > 0)
                .Select(p => new
                {
                    p.Descricao,
                    Funcionarios = p.Funcionarios.Select(f => f.Nome)
                })
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");

                foreach (var funcionario in departamento.Funcionarios)
                {
                    Console.WriteLine($"Nome: {funcionario}");
                }
            }
        }

        static void ConsultaParametrizada()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var id = 0;
            var departamentos = db.Departamentos
                .FromSqlRaw("SELECT * FROM Departamentos WHERE id > {0}", id)
                .Where(x => !x.Excluido)
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");
            }
        }

        static void ConsultaInterpolada()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var id = 0;
            var departamentos = db.Departamentos
                .FromSqlInterpolated($"SELECT * FROM Departamentos WHERE id > {id}")
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");
            }
        }

        static void ConsultaComTag()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var departamentos = db.Departamentos
                .TagWith("Estou enviando um comentário para o servidor")
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");
            }
        }

        static void EntendendoConsulta1NN1()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var departamentos = db.Departamentos
                .Include(x => x.Funcionarios)
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");

                foreach (var funcionario in departamento.Funcionarios)
                {
                    Console.WriteLine($"Nome: {funcionario.Nome}");
                }
            }

            var funcionarios = db.Funcionarios
                .Include(x => x.Departamento)
                .ToList();

            foreach (var funcionario in funcionarios)
            {
                Console.WriteLine($"Nome: {funcionario.Nome} | Departamento: {funcionario.Departamento.Descricao}");               
            }
        }

        static void DivisaoDeConsulta()
        {
            using var db = new ApplicationContext();
            Setup(db);

            var departamentos = db.Departamentos
                .Include(x => x.Funcionarios)
                .AsSplitQuery()
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");

                foreach (var funcionario in departamento.Funcionarios)
                {
                    Console.WriteLine($"Nome: {funcionario.Nome}");
                }
            }            
        }

        static void Setup(ApplicationContext db)
        {
            // db.Database.EnsureDeleted();

            if (db.Database.EnsureCreated())
            {
                db.Departamentos.AddRange(
                    new Departamento
                    {
                        Ativo = true,
                        Descricao = "Departamento 01",
                        Funcionarios = new System.Collections.Generic.List<Funcionario>{
                            new Funcionario{
                                CPF = "00",
                                RG = "00",
                                Nome = "Gustavo"
                            }
                        },
                        Excluido = true
                    },
                     new Departamento
                     {
                         Ativo = true,
                         Descricao = "Departamento 02",
                         Funcionarios = new System.Collections.Generic.List<Funcionario>{
                            new Funcionario{
                                CPF = "00",
                                RG = "00",
                                Nome = "Vitória"
                            },
                            new Funcionario{
                                CPF = "00",
                                RG = "00",
                                Nome = "Eduarda"
                            }
                        }
                     }
                );

                db.SaveChanges();
                db.ChangeTracker.Clear();
            }
        }
    }
}
