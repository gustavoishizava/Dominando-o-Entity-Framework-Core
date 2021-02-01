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
            AplicarMigracaoEmtempoExecucao();
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
    }
}
