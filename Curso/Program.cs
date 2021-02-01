using System.Diagnostics;
using System;
using Curso.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;

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
            new ApplicationContext().Departamentos.AsNoTracking().Any();
            _count = 0;
            GerenciarEstadoDaConexao(false);
            _count = 0;
            GerenciarEstadoDaConexao(true);
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
    }
}
