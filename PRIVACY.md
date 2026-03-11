<!DOCTYPE html>
<html lang="en">
<body>

<h1>Privacy Policy</h1>

<p><strong>Effective Date:</strong> March 11, 2026</p>

<p>
This Privacy Policy describes how Reactor collects, uses,
and stores information when used within a Valour server.
</p>

<hr>

<h2>1. Information Collected</h2>

<p>
Reactor collects only the minimum data required to function.
</p>

<h3>Information Stored (Persisted to Disk)</h3>
<ul>
    <li>Planet IDs</li>
    <li>Channel IDs</li>
    <li>Message IDs of reaction role messages</li>
    <li>Emoji-to-role mappings</li>
    <li>Confirmation message delete delay settings</li>
</ul>

<h3>Information Not Stored</h3>
<ul>
    <li>Message content (beyond the initial reaction message text provided by the administrator)</li>
    <li>User IDs (beyond what is transiently used to assign or remove roles during a reaction event)</li>
    <li>Direct Messages (DMs)</li>
    <li>User credentials</li>
    <li>Email addresses</li>
    <li>Passwords</li>
    <li>IP addresses</li>
    <li>Analytics or tracking data</li>
</ul>

<hr>

<h2>2. Purpose of Data Collection</h2>

<p>Stored information is used exclusively to:</p>

<ol>
    <li>Track which messages are configured as reaction role messages</li>
    <li>Map emoji reactions to planet roles</li>
    <li>Assign or remove roles from members when they react or unreact to a tracked message</li>
    <li>Automatically clean up stale data when messages are deleted</li>
</ol>

<p>
Data is not used for marketing, advertising, profiling, or analytics.
</p>

<hr>

<h2>3. Data Storage and Security</h2>

<p>
All data is stored in a local SQLite database file (<code>reactor.db</code>) on the hosting server.
</p>

<p>
The operator of the self-hosted instance is responsible for securing the hosting environment
and the database file.
</p>

<p>
Stale or deleted reaction messages are automatically removed from the database on bot startup.
</p>

<hr>

<h2>4. Third-Party Processing</h2>

<p>
Reactor does not transmit any data to external services beyond the Valour.gg API,
which is required for core bot functionality such as assigning roles and sending messages.
</p>

<p>
Reactor does not use any external AI providers, analytics services, or cloud storage.
</p>

<hr>

<h2>5. Data Retention</h2>

<p>
Reaction role message data is retained in the SQLite database until:
</p>

<ul>
    <li>The reaction message is deleted from Valour (automatic cleanup on bot startup)</li>
    <li>An administrator runs the <code>r.delete</code> command</li>
    <li>The database file is manually deleted by the host operator</li>
</ul>

<hr>

<h2>6. Self-Hosted Responsibility</h2>

<p>
Reactor is designed for self-hosting.
</p>

<p>
The hosting operator is responsible for:
</p>

<ul>
    <li>Server security</li>
    <li>Network configuration</li>
    <li>Bot token protection</li>
    <li>Database file security</li>
    <li>Compliance with applicable laws</li>
</ul>

<hr>

<h2>7. Changes to This Policy</h2>

<p>
If data collection practices change in future versions,
this Privacy Policy will be updated prior to implementation.
</p>

<p>
Continued use of the Bot after updates constitutes acceptance
of the revised policy.
</p>

<hr>

<h2>8. Contact Information</h2>

<p>
For privacy-related inquiries:
</p>

<p>
Email: contact@skyjoshua.xyz
</p>

</body>
</html>