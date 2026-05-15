# APPLICATION
A client-server application that records activity on the user's PC and allows you to analyze what the user was doing and when.
The client-side component is a WinForms application that typically runs in the background and periodically determines which process is currently active and the title of its window. It also records when the PC is turned on or off. The application window (usually hidden in the system tray) includes a DataGridView displaying the information collected so far. When the currently active process changes (after all, we don’t switch between windows every second), a set of data (date, time, process name, window title) is recorded locally (in the application’s own database, using SQLite) and sent to the server.
The server-side component is necessary because a user may have many work PCs—and it makes sense for them to track their activity across all of them. Plus, our "software suite" may have many users, each with their own PCs running the client components of this application. Hence the server-side component, which supports multi-user functionality.
The server is built using PHP (Laravel) and PostgreSQL.
Development of the client-side is currently underway.
# COMMIT MESSAGE
Write commits only as a list with a '-' separator at the beginning, each change on a new line and in lowercase.
Do not write changes in each file if you can logically group changes from several files. Use the English language.