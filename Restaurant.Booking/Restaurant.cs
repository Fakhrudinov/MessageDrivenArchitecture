﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Lesson1;

namespace Restaurant.Booking
{
    public class Restaurant
    {
        private readonly List<Table> _tables = new ();
        private System.Timers.Timer _timerResetTablesBooking;

        public Restaurant()
        {
            for (ushort i = 1; i <= 10; i++)
            {
                _tables.Add(new Table(i));
            }

            _timerResetTablesBooking = new System.Timers.Timer(120_000);
            _timerResetTablesBooking.AutoReset = true;
            _timerResetTablesBooking.Elapsed += new ElapsedEventHandler(ResetAllTablesBooking);
            _timerResetTablesBooking.Start();
        }

        public async Task<bool?> BookFreeTableAsync(int countOfPersons, Guid orderId, CancellationToken token = default)
        {
            Console.WriteLine($"Спасибо за Ваше обращение, я подберу столик и подтвержу вашу бронь #{orderId}," +
                              "\r\nВам придет уведомление");

            var table = _tables.FirstOrDefault(t => t.SeatsCount > countOfPersons
                                                        && t.State == EnumState.Free);
            await Task.Delay(1000 * 1); //у нас нерасторопные менеджеры, 5 секунд они находятся в поисках стола
            
            return table?.SetState(EnumState.Booked, orderId);
        }

        public void CancelReservationAsync(Guid orderId, CancellationToken token = default)
        {
            Task.Run(async () =>
            {
                var table = _tables.Where(t => t.OrderId == orderId).FirstOrDefault();

                await Task.Delay(1000 * 1, token).ConfigureAwait(true);

                table?.SetState(EnumState.Free);
            }, token);
        }

        internal async Task DeleteBookingForTableAsync(int tableNumber)
        {
            Console.WriteLine($"\tСнимем асинхронно бронь сo столика {tableNumber} и оповестим вас");

            await Task.Run(async () =>
            {
                Table table = _tables.FirstOrDefault(t => t.Id == tableNumber);
                await Task.Delay(2000);

                if (table is null)
                {
                    InformManagementAboutNullProblem("Снятие брони асинхронно", tableNumber);
                }
                else if (table.State == EnumState.Free)
                {
                    Console.WriteLine($"Да этот стол #{tableNumber} и так свободен был, что вы нас от работы отвлекаете!");
                }
                else
                {
                    bool isSucces = table.SetState(EnumState.Free);

                    Console.WriteLine($"Снятие брони с стола номер {table.Id} = {isSucces}");
                }
            });
        }

        private void InformManagementAboutNullProblem(string action, int tableNumber)
        {
            Console.WriteLine($"Внимание! Что-то пошло не так при выполнении '{action}' для стола #{tableNumber}. " +
                $"Похоже у нас украли стол, так как вернулся null...");
        }

        internal void PrintTablesStatus()
        {
            Console.WriteLine("\tСостояние столиков:");
            foreach (Table table in _tables)
            {
                Console.WriteLine($"\t\tСтол#{table.Id} {table.State} \t{table.OrderId}");
            }
        }

        private async void ResetAllTablesBooking(object? sender, ElapsedEventArgs e)
        {
            await Task.Run(async () =>
            {
                Console.WriteLine("Автоматическое снятие бронирования со всех столов");

                foreach (Table table in _tables)
                {
                    if (table.State == EnumState.Booked)
                    {
                        bool isSucces = table.SetState(EnumState.Free);
                        Console.WriteLine($"УВЕДОМЛЕНИЕ асинхронно! Снятие брони с стола номер {table.Id} = {isSucces}");
                    }
                }
            });
        }
    }
}