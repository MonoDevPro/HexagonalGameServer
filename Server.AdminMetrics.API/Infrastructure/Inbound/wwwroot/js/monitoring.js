/**
 * Painel de Monitoramento do Servidor
 * Script para buscar e exibir dados do servidor em tempo real
 */

// Configuração
const CONFIG = {
    refreshInterval: 10000, // Atualiza a cada 10 segundos
    apiBasePath: '/api/ServerMonitoring',
    endpoints: {
        status: '/status',
        players: '/players',
        metrics: '/metrics'
    }
};

// Cache para dados das zonas
let zoneList = new Set();

// Inicializar o painel
document.addEventListener('DOMContentLoaded', () => {
    // Inicializar atualizações periódicas
    updateAllData();
    setInterval(updateAllData, CONFIG.refreshInterval);

    // Configurar filtros de jogadores
    const searchInput = document.getElementById('player-search');
    const zoneFilter = document.getElementById('zone-filter');
    
    searchInput.addEventListener('input', filterPlayers);
    zoneFilter.addEventListener('change', filterPlayers);
});

// Atualizar todos os dados do painel
async function updateAllData() {
    try {
        await Promise.all([
            updateServerStatus(),
            updateServerMetrics(),
            updatePlayersList()
        ]);
        
        // Atualizar timestamp da última atualização
        const now = new Date();
        document.getElementById('last-update').textContent = now.toLocaleTimeString();
    } catch (error) {
        console.error('Erro ao atualizar os dados:', error);
    }
}

// Atualizar dados do status do servidor
async function updateServerStatus() {
    try {
        const response = await fetch(`${CONFIG.apiBasePath}${CONFIG.endpoints.status}`);
        if (!response.ok) throw new Error('Falha ao buscar status do servidor');
        
        const data = await response.json();
        
        // Atualizar indicador de status
        const statusDot = document.getElementById('status-dot');
        const statusText = document.getElementById('status-text');
        
        statusDot.className = data.isOnline ? 'dot online' : 'dot offline';
        statusText.textContent = data.isOnline ? 'Online' : 'Offline';
        
        // Atualizar informações de versão
        document.getElementById('server-version').textContent = data.version;
        document.getElementById('server-env').textContent = data.environment;
        
        // Atualizar detalhes
        document.getElementById('start-time').textContent = new Date(data.startTime).toLocaleString();
        document.getElementById('uptime').textContent = formatUptime(data.uptime);
        document.getElementById('connected-players').textContent = data.connectedPlayers;
        
    } catch (error) {
        console.error('Erro ao atualizar status:', error);
    }
}

// Atualizar métricas do servidor
async function updateServerMetrics() {
    try {
        const response = await fetch(`${CONFIG.apiBasePath}${CONFIG.endpoints.metrics}`);
        if (!response.ok) throw new Error('Falha ao buscar métricas do servidor');
        
        const data = await response.json();
        
        // Atualizar barras de progresso
        updateProgressBar('cpu-bar', 'cpu-value', data.cpuUsage, '%');
        updateProgressBar('memory-bar', 'memory-value', data.memoryUsageMb, ' MB');
        
        // Atualizar contadores
        document.getElementById('threads-value').textContent = data.activeThreads;
        document.getElementById('requests-value').textContent = data.requestsPerMinute;
        document.getElementById('connections-value').textContent = data.connectionsPerMinute;
        
    } catch (error) {
        console.error('Erro ao atualizar métricas:', error);
    }
}

// Atualizar lista de jogadores
async function updatePlayersList() {
    try {
        const response = await fetch(`${CONFIG.apiBasePath}${CONFIG.endpoints.players}`);
        if (!response.ok) throw new Error('Falha ao buscar lista de jogadores');
        
        const players = await response.json();
        const playersTable = document.getElementById('players-list');
        
        // Limpar a tabela existente
        playersTable.innerHTML = '';
        
        // Limpar e atualizar lista de zonas
        zoneList.clear();
        
        if (players.length === 0) {
            const row = document.createElement('tr');
            row.innerHTML = `<td colspan="4" class="no-players">Nenhum jogador online</td>`;
            playersTable.appendChild(row);
        } else {
            // Adicionar cada jogador à tabela
            players.forEach(player => {
                const row = document.createElement('tr');
                row.setAttribute('data-player-id', player.playerId);
                row.setAttribute('data-zone', player.currentZone);
                
                row.innerHTML = `
                    <td>${player.playerId}</td>
                    <td>${player.username}</td>
                    <td>${formatTimeSince(player.connectedSince)}</td>
                    <td>${player.currentZone}</td>
                `;
                
                playersTable.appendChild(row);
                
                // Adicionar zona ao conjunto de zonas
                zoneList.add(player.currentZone);
            });
            
            // Atualizar filtro de zonas
            updateZoneFilter();
        }
        
    } catch (error) {
        console.error('Erro ao atualizar lista de jogadores:', error);
    }
}

// Atualizar filtro de zonas
function updateZoneFilter() {
    const zoneFilter = document.getElementById('zone-filter');
    const currentSelection = zoneFilter.value;
    
    // Manter apenas a opção "Todas as Zonas"
    zoneFilter.innerHTML = '<option value="">Todas as Zonas</option>';
    
    // Adicionar cada zona como uma opção
    Array.from(zoneList).sort().forEach(zone => {
        const option = document.createElement('option');
        option.value = zone;
        option.textContent = zone;
        
        if (zone === currentSelection) {
            option.selected = true;
        }
        
        zoneFilter.appendChild(option);
    });
}

// Filtrar jogadores por pesquisa e zona
function filterPlayers() {
    const searchInput = document.getElementById('player-search').value.toLowerCase();
    const zoneFilter = document.getElementById('zone-filter').value;
    
    const rows = document.querySelectorAll('#players-list tr');
    
    rows.forEach(row => {
        const playerId = row.querySelector('td:first-child')?.textContent.toLowerCase() || '';
        const username = row.querySelector('td:nth-child(2)')?.textContent.toLowerCase() || '';
        const zone = row.getAttribute('data-zone') || '';
        
        const matchesSearch = playerId.includes(searchInput) || username.includes(searchInput);
        const matchesZone = !zoneFilter || zone === zoneFilter;
        
        row.style.display = (matchesSearch && matchesZone) ? '' : 'none';
    });
}

// Utilitários
function updateProgressBar(barId, valueId, value, unit) {
    const bar = document.getElementById(barId);
    const valueElement = document.getElementById(valueId);
    
    // Ajustar a barra de progresso (máximo de 100%)
    const percentage = Math.min(value, 100);
    bar.style.width = `${percentage}%`;
    
    // Atualizar o texto do valor
    valueElement.textContent = `${Math.round(value * 10) / 10}${unit}`;
    
    // Ajustar cor com base no valor
    if (value > 80) {
        bar.style.backgroundColor = '#e74c3c'; // Vermelho para alto uso
    } else if (value > 60) {
        bar.style.backgroundColor = '#f39c12'; // Laranja para uso médio-alto
    } else {
        bar.style.backgroundColor = '#2ecc71'; // Verde para uso normal
    }
}

function formatUptime(uptimeString) {
    // Formata o uptime a partir da string da API
    try {
        const match = uptimeString.match(/^([^.]*)/);
        if (match) return match[0].replace('d', 'd '); // Formata dias
        return uptimeString;
    } catch (err) {
        return uptimeString;
    }
}

function formatTimeSince(dateString) {
    try {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        
        if (diffMins < 1) return 'Agora mesmo';
        if (diffMins === 1) return '1 minuto atrás';
        if (diffMins < 60) return `${diffMins} minutos atrás`;
        
        const diffHours = Math.floor(diffMins / 60);
        if (diffHours === 1) return '1 hora atrás';
        if (diffHours < 24) return `${diffHours} horas atrás`;
        
        return date.toLocaleString();
    } catch (err) {
        return dateString;
    }
}
