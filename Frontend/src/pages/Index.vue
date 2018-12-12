<template>
    <q-page class="flex flex-center">
        <label for="ip-address">Device IP</label>
        <input type="text" id="ip-address" v-model="ip">
        <label for="port">Device Port</label>
        <input type="text" id="port" v-model="port">
        <input type="button" value="Connect" @click="connect">
    </q-page>
</template>

<script>
    import io from 'socket.io-client';

    export default {
      data () {
        return {
          ip: 'razaman2.dyndns.org',
          port: 41733
        }
      },
      methods: {
        connect () {
          let connection = io.connect(`ws://${this.ip}:${this.port}`, {
            transports: ['websocket'],
            upgrade: false,
            // forceNew: true,
            // // rejectUnauthorized: false,
            reconnection: false,
            // jsonp: false,
            path: '/'
          });
          console.log(connection);
        }
      }
    }
</script>

<style>
    #ip-address, #port {
        border: none;
        margin: 0 10px;
    }
</style>
