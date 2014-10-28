<?php
require_once('includes/header.inc.php');
$ref = '{pegasus.xxlwebhosting.nl:993/imap/ssl}';
$url = $ref.'INBOX';
echo '<pre>';
$mailbox = imap_open ( $url, 'nap@hcnobel.nl' , 'ERz2PpTZ8hZL') or die("IMAP can't connect: " . imap_last_error());
$list = imap_getmailboxes($mailbox ,$ref,'*');
if (is_array($list)) {
    foreach ($list as $key => $val) {
        echo "($key) ";
        echo imap_utf7_decode($val->name) . ",";
        echo "'" . $val->delimiter . "',";
        echo $val->attributes . "<br />\n";
    }
} else {
    echo "imap_getmailboxes failed: " . imap_last_error() . "\n";
}
$quota_values = imap_get_quotaroot($mailbox, "INBOX") or die("imap_get_quotaroot error: " . imap_last_error());
if (is_array($quota_values)) {
   $storage = $quota_values['STORAGE'];
   echo "STORAGE usage level is: " .  $storage['usage']."\n";
   echo "STORAGE limit level is: " .  $storage['limit']."\n";

   $message = $quota_values['MESSAGE'];
   echo "MESSAGE usage level is: " .  $message['usage']."\n";
   echo "MESSAGE limit is: " .  $message['limit']."\n";

   /* ...  */
}
$list = imap_list($mailbox, $ref, "*");
if (is_array($list)) {
    foreach ($list as $val) {
        echo imap_utf7_decode($val) . "\n";
    }
} else {
    echo "imap_list failed: " . imap_last_error() . "\n";
}
$num = imap_num_msg ($mailbox);
echo "$num messages in mailbox\n";
for($i=1; $i<=$num; $i++) {
	  //read that mail recently arrived
	  echo "Message #$i\n";
	  echo "Headers\n";
	  $h = imap_headerinfo($mailbox,$i);
	  var_dump($h);
	  echo "Body\n";
	  echo imap_qprint(imap_body($mailbox, $i));
 } 
imap_close($mailbox);
echo '</pre>';
?>