<!DOCTYPE html>
<html lang="en">
<body>

<h1>Reactor</h1>

<p>
Reactor is a Valour.gg bot that enables server administrators to create reaction role messages.
Users can react to a message with an emoji to automatically be assigned or removed from a role.
</p>

<hr>

<h2>Features</h2>
<ul>
    <li>Create reaction role messages in any planet channel</li>
    <li>Map emojis to roles — reacting adds the role, removing the reaction removes it</li>
    <li>Automatic cleanup of deleted reaction messages from the database</li>
    <li>Persistent storage via SQLite — survives bot restarts</li>
    <li>Permission-based command access (Manage Roles or Full Control required)</li>
    <li>Built with .NET and the Valour SDK</li>
    <li>Open-source under AGPL-3.0</li>
</ul>

<hr>

<h2>How It Works</h2>
<p>Reactor connects to the Valour.gg API and listens for reactions on configured messages.</p>
<ul>
    <li>Reaction role messages and their emoji-to-role mappings are stored in a local SQLite database</li>
    <li>When a user reacts to a tracked message, Reactor assigns the mapped role</li>
    <li>When a user removes their reaction, Reactor removes the mapped role</li>
    <li>Stale messages (deleted from Valour) are automatically pruned from the database on startup</li>
</ul>

<hr>

<h2>Requirements</h2>
<ul>
    <li>.NET 10+</li>
    <li>Valid Valour bot token</li>
</ul>

<hr>

<h2>Installation</h2>

<pre><code>fork the project
git clone https://github.com/YOUR_USERNAME/Reactor.git
cd Reactor
dotnet restore</code></pre>

<p>
All required NuGet packages are installed automatically via the <code>.csproj</code> file.
</p>

<hr>

<h2>Configuration</h2>

<p>Create a <code>.env</code> file in the root directory:</p>

<pre><code>TOKEN=your-valour-bot-token</code></pre>

<p>
Do not commit this file to version control.
</p>

<hr>

<h2>Running the Bot</h2>

<pre><code>dotnet run</code></pre>

<hr>

<h2>Commands</h2>

<p>All commands require <strong>Manage Roles</strong> or <strong>Full Control</strong> permissions, except <code>r.help</code> and <code>r.source</code>.</p>

<table border="1" cellpadding="6">
<tr>
<th>Command</th>
<th>Description</th>
</tr>
<tr>
<td><code>r.help</code></td>
<td>Shows the list of available commands</td>
</tr>
<tr>
<td><code>r.source</code></td>
<td>Shows the source code of the bot</td>
</tr>
<tr>
<td><code>r.create &lt;message text&gt;</code></td>
<td>Creates a new reaction role message in the current channel</td>
</tr>
<tr>
<td><code>r.add &lt;messageId&gt; &lt;emoji&gt; &lt;roleId&gt;</code></td>
<td>Maps an emoji to a role on a reaction message</td>
</tr>
<tr>
<td><code>r.remove &lt;messageId&gt; &lt;emoji&gt;</code></td>
<td>Removes an emoji-to-role mapping from a reaction message</td>
</tr>
<tr>
<td><code>r.delete &lt;messageId&gt;</code></td>
<td>Deletes a reaction message and all its role mappings</td>
</tr>
</table>

<hr>

<h2>Data Storage</h2>

<p>Reactor stores the following data in a local SQLite database (<code>reactor.db</code>):</p>
<ul>
    <li>Reaction message IDs, channel IDs, and planet IDs</li>
    <li>Emoji-to-role mappings per reaction message</li>
    <li>Configurable delete delay for confirmation messages (default: 5 seconds)</li>
</ul>

<p>
Full privacy policy:<br>
<a href="https://github.com/SkyJoshua/Reactor/blob/main/PRIVACY.md">
https://github.com/SkyJoshua/Reactor/blob/main/PRIVACY.md
</a>
</p>

<hr>

<h2>License</h2>
<p>
This project is licensed under the
<strong>GNU Affero General Public License v3.0 (AGPL-3.0)</strong>.
</p>

<p>
See the LICENSE file for details:<br>
<a href="https://github.com/SkyJoshua/Reactor/blob/main/LICENSE">
https://github.com/SkyJoshua/Reactor/blob/main/LICENSE
</a>
</p>

<p>
If you modify and deploy this project publicly (including as a hosted service),
you must make your source code available under the same AGPL-3.0 license.
</p>

<hr>

<h2>Contributing</h2>

<p>
Contributions are welcome. By submitting a contribution, you agree that your
contributions will be licensed under AGPL-3.0.
</p>

<ol>
    <li>Fork the repository</li>
    <li>Create a feature branch</li>
    <li>Submit a pull request</li>
</ol>

</body>
</html>