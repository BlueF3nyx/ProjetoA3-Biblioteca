using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using BibliotecaAPP.Models;

namespace BibliotecaAPP.Views
{
    public partial class HistoricoEmprestimo : ContentPage
    {
        public ObservableCollection<Historico> HistoricoItems { get; set; }

        public HistoricoEmprestimo()
        {
            InitializeComponent();

            // Simulação de dados do histórico
            HistoricoItems = new ObservableCollection<Historico>
            {
                new Historico
                {
                    Id_Livro = "Curso Extensivo de Python",
                    Id_Membro = "João da Silva (ID: 00123)",
                    DataEmprestimo = new DateTime(2025, 5, 10),
                    DataDevolucao = new DateTime(2025, 5, 26),
                    Status = "Devolvido",
                    StatusColor = Colors.Green
                },
                new Historico
                {
                    Id_Livro = "Aprenda C# do Básico ao Avançado",
                    Id_Membro = "Maria Oliveira (ID: 00124)",
                    DataEmprestimo = new DateTime(2025, 5, 12),
                    DataDevolucao = new DateTime(2025, 5, 28),
                    Status = "Atrasado",
                    StatusColor = Colors.Orange
                },
                new Historico
                { 
                    Id_Livro = "Introdução ao Banco de Dados",
                    Id_Membro = "Carlos Souza (ID: 00125)",
                    DataEmprestimo = new DateTime(2025, 5, 15),
                    DataDevolucao = new DateTime(2025, 5, 30),
                    Status = "Perdido",
                    StatusColor = Colors.Red
                }
            };

            historicoCollectionView.ItemsSource = HistoricoItems;
        }

        
    }
}
