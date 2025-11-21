import { Room, Client } from "colyseus";
import { Schema, type, MapSchema } from "@colyseus/schema";

export class Player extends Schema {
    @type("uint8")
    loss = 0;

    @type("int8")
    maxHP = 0;

    @type("int8")
    currentHP = 0;

    @type("number")
    speed = 0;

    @type("number")
    pX = 0;

    @type("number")
    pY = 0;

    @type("number")
    pZ = 0;

    @type("number")
    vX = 0;

    @type("number")
    vY = 0;

    @type("number")
    vZ = 0;

    @type("number")
    rX = 0;

    @type("number")
    rY = 0;   
}

export class State extends Schema {
    @type({ map: Player })
    players = new MapSchema<Player>();

    something = "This attribute won't be sent to the client-side";

    createPlayer(sessionId: string, data: any) {
        const player = new Player();

        player.maxHP = data.hp;
        player.currentHP = data.hp;
        player.speed = data.speed;
        player.pX = data.pX;
        player.pX = data.pX;
        player.pY = data.pY;
        player.pZ = data.pZ;
        player.rY = data.rY;
        
        this.players.set(sessionId, player);
    }

    removePlayer(sessionId: string) {
        this.players.delete(sessionId);
    }

    movePlayer (sessionId: string, data: any) {
        const player = this.players.get(sessionId);
        player.pX = data.pX;
        player.pY = data.pY;
        player.pZ = data.pZ;
        player.vX = data.vX;
        player.vY = data.vY;
        player.vZ = data.vZ;
        player.rX = data.rX;
        player.rY = data.rY;             
    }

    respawnPlayer(sessionId: string, spawnPoint: { x: number, y: number, z: number }) {
        const player = this.players.get(sessionId);
        if (!player) return;
        
        player.currentHP = player.maxHP;
        player.pX = spawnPoint.x;
        player.pY = spawnPoint.y;
        player.pZ = spawnPoint.z;
        player.rY = 0; // или другое значение по умолчанию
    }

}

export class StateHandlerRoom extends Room<State> {
    maxClients = 2;
    spawnPointCount = 1;


    onCreate (options) {
        this.spawnPointCount = options.points;
        console.log("StateHandlerRoom created!", options);

        this.setState(new State());
        //this.setPatchRate(10);
        //console.log("!", this.patchRate);
        this.onMessage("move", (client, data) => {
            //console.log("StateHandlerRoom received message from", client.sessionId, ":", data);
            this.state.movePlayer(client.sessionId, data);
        });
        this.onMessage("shoot", (client, data) => {

            this.broadcast("Shoot", data, {except: client});
        });
        this.onMessage("damage", (client, data) => {
            const clientID = data.id;
            const player = this.state.players.get(clientID);
            
            let hp = player.currentHP - data.value;
            
            if(hp > 0){
                player.currentHP = hp;
                return;
            }
            
            player.loss++;
            player.currentHP = player.maxHP;

            for(var i = 0; i < this.clients.length; i++){

                if(this.clients[i].id != clientID) continue;

                const point = Math.floor(Math.random() * this.spawnPointCount);

                this.clients[i].send("restart", point);
            }
        });

    }

    onAuth(client, options, req) {
        return true;
    }

    onJoin (client: Client, data: any) {
        if(this.clients.length > 1) this.lock();
        client.send("hello", "world");
        this.state.createPlayer(client.sessionId, data);
    }

    onLeave (client) {
        this.state.removePlayer(client.sessionId);
    }

    onDispose () {
        console.log("Dispose StateHandlerRoom");
    }
}