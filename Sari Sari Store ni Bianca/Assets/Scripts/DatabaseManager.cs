using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    private string dbName = "URI=file:SariSariDatabase.db";

    void Awake()
    {
        // Set path to the persistent data directory so it saves outside the Unity Editor
        string filepath = Path.Combine(Application.persistentDataPath, "SariSariDatabase.db");
        dbName = "URI=file:" + filepath;

        InitializeDatabaseSchema();
    }

    // --- RUBRIC 1: CORE DATA ENTITIES (SCHEMA) ---
    private void InitializeDatabaseSchema()
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // 1. Player Profile Table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS PlayerProfile (
                        PlayerID TEXT PRIMARY KEY,
                        Currency INTEGER,
                        RemainingTime REAL
                    );";
                command.ExecuteNonQuery();

                // 2. Transaction Logs Table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS TransactionLogs (
                        LogID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PlayerID TEXT,
                        ProfitChange INTEGER,
                        Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";
                command.ExecuteNonQuery();

                // 3. Inventory Table (Linking Player to Items Sold/Owned)
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Inventory (
                        RecordID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PlayerID TEXT,
                        ItemName TEXT,
                        CostPrice INTEGER,
                        RetailPrice INTEGER
                    );";
                command.ExecuteNonQuery();
            }
        }
    }

    // --- RUBRIC 3: SNAPSHOT LOADING ---
    public PlayerModel LoadPlayerProfile(string playerID)
    {
        PlayerModel loadedPlayer = null;

        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM PlayerProfile WHERE PlayerID = @id;";
                command.Parameters.Add(new SqliteParameter("@id", playerID));

                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        loadedPlayer = new PlayerModel
                        {
                            PlayerID = reader["PlayerID"].ToString(),
                            Currency = Convert.ToInt32(reader["Currency"]),
                            RemainingTime = Convert.ToSingle(reader["RemainingTime"])
                        };
                    }
                }

                // If no player exists, create the default starter profile
                if (loadedPlayer == null)
                {
                    loadedPlayer = new PlayerModel { PlayerID = playerID, Currency = 0, RemainingTime = 90f };

                    command.CommandText = "INSERT INTO PlayerProfile (PlayerID, Currency, RemainingTime) VALUES (@id, 0, 90);";
                    command.ExecuteNonQuery();
                }
            }
        }
        return loadedPlayer;
    }

    // --- RUBRIC 2: FUNCTIONAL REQUIREMENTS (ATOMICITY & TRANSACTIONS) ---
    // This executes the "Buy/Sell" process as a single unit of work.
    public bool ProcessSaleTransaction(string playerID, int netProfit, List<ItemData> soldItems)
    {
        bool transactionSuccess = false;

        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            // BEGIN TRANSACTION: Locks the database rows
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;

                        // Step 1: Update Player Currency
                        command.CommandText = "UPDATE PlayerProfile SET Currency = Currency + @profit WHERE PlayerID = @id;";
                        command.Parameters.Add(new SqliteParameter("@profit", netProfit));
                        command.Parameters.Add(new SqliteParameter("@id", playerID));
                        command.ExecuteNonQuery();

                        // Step 2: Write to Transaction Log
                        command.CommandText = "INSERT INTO TransactionLogs (PlayerID, ProfitChange) VALUES (@id, @profit);";
                        command.ExecuteNonQuery();

                        // Step 3: Write individual items to the Inventory Table
                        command.CommandText = "INSERT INTO Inventory (PlayerID, ItemName, CostPrice, RetailPrice) VALUES (@id, @name, @cost, @retail);";

                        // We use parameters in a loop to securely insert every item sold
                        command.Parameters.Add(new SqliteParameter("@name", DbType.String));
                        command.Parameters.Add(new SqliteParameter("@cost", DbType.Int32));
                        command.Parameters.Add(new SqliteParameter("@retail", DbType.Int32));

                        foreach (ItemData item in soldItems)
                        {
                            command.Parameters["@name"].Value = item.itemName;
                            command.Parameters["@cost"].Value = item.costPrice;
                            command.Parameters["@retail"].Value = item.retailPrice;
                            command.ExecuteNonQuery();
                        }
                    }

                    // COMMIT: If code reaches here without errors, permanently save everything
                    transaction.Commit();
                    transactionSuccess = true;
                }
                catch (Exception e)
                {
                    // ROLLBACK: If ANY step fails (e.g. power loss, crash), revert everything back to how it was
                    Debug.LogError("Transaction Failed. Rolling back database. Error: " + e.Message);
                    transaction.Rollback();
                    transactionSuccess = false;
                }
            }
        }

        return transactionSuccess;
    }

    // --- UTILITY UPDATES ---
    public void UpdateCurrency(string playerID, int profitChange)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE PlayerProfile SET Currency = Currency + @change WHERE PlayerID = @id;";
                command.Parameters.Add(new SqliteParameter("@change", profitChange));
                command.Parameters.Add(new SqliteParameter("@id", playerID));
                command.ExecuteNonQuery();
            }
        }
    }

    public void UpdateTime(string playerID, float remainingTime)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE PlayerProfile SET RemainingTime = @time WHERE PlayerID = @id;";
                command.Parameters.Add(new SqliteParameter("@time", remainingTime));
                command.Parameters.Add(new SqliteParameter("@id", playerID));
                command.ExecuteNonQuery();
            }
        }
    }
}